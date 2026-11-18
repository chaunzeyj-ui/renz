using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PrintConnect.domain.entities;
using PrintConnect.domain.Entities;
using PrintConnect.infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup Master CRM Database (M DB)
builder.Services.AddDbContext<MasterCrmDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterCrm")));

// 2. Setup Identity for Logins
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<MasterCrmDbContext>();

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// 3. Register the Tenant Factory
builder.Services.AddScoped<ITenantCrmDbContextFactory, TenantCrmDbContextFactory>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapIdentityApi<IdentityUser>();
app.UseAuthorization();
app.MapControllers();

// ==========================================
// PRINT CONNECT CRM ENDPOINTS
// ==========================================

// --- MASTER DB ENDPOINTS ---

// 1. Create a new Company
app.MapPost("/companies", async (
    CreateCompanyRequest request,
    MasterCrmDbContext db) =>
{
    var company = request.Company;
    var companyDb = request.CompanyDb;

    db.Companies.Add(company);
    await db.SaveChangesAsync();

    companyDb.CompanyId = company.CompanyId;
    db.CompanyDatabases.Add(companyDb);
    await db.SaveChangesAsync();

    return Results.Ok(new { Message = "Company created successfully!", CompanyId = company.CompanyId });
});

// 2. Add a Device (e.g., Printer, PC) to a Company
app.MapPost("/devices", async (
    Device device,
    MasterCrmDbContext db) =>
{
    db.Devices.Add(device);
    await db.SaveChangesAsync();
    return Results.Ok(new { Message = "Device added successfully!", DeviceId = device.DeviceId });
});


// --- TENANT DB ENDPOINTS ---

// 3. Add a Product
app.MapPost("/tenant/{companyId:int}/products", async (
    int companyId, Product product, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.Products.Add(product);
    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = "Product added successfully!", ProductId = product.ProductId });
});

// 4. Get all Products
app.MapGet("/tenant/{companyId:int}/products", async (
    int companyId, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    return Results.Ok(await tenantDb.Products.ToListAsync());
});

// 5. Submit an Inquiry/Complaint
app.MapPost("/tenant/{companyId:int}/inquiries", async (
    int companyId, Inquiry inquiry, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.Inquiries.Add(inquiry);
    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = $"{inquiry.Type} submitted successfully!", InquiryId = inquiry.InquiryId });
});

// 6. Get all Inquiries/Complaints
app.MapGet("/tenant/{companyId:int}/inquiries", async (
    int companyId, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    return Results.Ok(await tenantDb.Inquiries.OrderByDescending(x => x.CreatedAt).ToListAsync());
});

// 7. Add a new Customer
app.MapPost("/tenant/{companyId:int}/customers", async (
    int companyId, Customer customer, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.Customers.Add(customer);
    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = "Customer added successfully!", CustomerId = customer.CustomerId });
});

// 8. Get all Customers
app.MapGet("/tenant/{companyId:int}/customers", async (
    int companyId, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    return Results.Ok(await tenantDb.Customers.ToListAsync());
});

// 9. Create a new Order (Cashier checkout)
app.MapPost("/tenant/{companyId:int}/orders", async (
    int companyId, CreateOrderRequest request, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);

    var customer = await tenantDb.Customers.FindAsync(request.CustomerId);
    if (customer == null) return Results.BadRequest("Customer not found.");

    var order = new Order
    {
        CustomerId = request.CustomerId,
        OrderDate = DateTime.UtcNow,
        Status = "Paid"
    };

    decimal totalAmount = 0;

    foreach (var item in request.Items)
    {
        var product = await tenantDb.Products.FindAsync(item.ProductId);
        if (product == null) return Results.BadRequest($"Product with ID {item.ProductId} not found.");

        var orderItem = new OrderItem
        {
            ProductId = product.ProductId,
            Quantity = item.Quantity,
            UnitPrice = product.UnitPrice
        };

        totalAmount += (product.UnitPrice * item.Quantity);
        order.OrderItems.Add(orderItem);
    }

    order.TotalAmount = totalAmount;

    tenantDb.Orders.Add(order);
    await tenantDb.SaveChangesAsync();

    return Results.Ok(new { Message = "Order created successfully!", OrderId = order.OrderId, TotalAmount = order.TotalAmount });
});

// 10. Get all Orders
app.MapGet("/tenant/{companyId:int}/orders", async (
    int companyId, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    var orders = await tenantDb.Orders
        .Include(o => o.Customer)
        .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
        .OrderByDescending(o => o.OrderDate)
        .ToListAsync();
    return Results.Ok(orders);
});

// 11. Add a Supplier
app.MapPost("/tenant/{companyId:int}/suppliers", async (
    int companyId, Supplier supplier, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.Suppliers.Add(supplier);
    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = "Supplier added successfully!", SupplierId = supplier.SupplierId });
});

// 12. Get all Suppliers
app.MapGet("/tenant/{companyId:int}/suppliers", async (
    int companyId, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    return Results.Ok(await tenantDb.Suppliers.ToListAsync());
});

// 13. Add/Update Inventory
app.MapPost("/tenant/{companyId:int}/inventory", async (
    int companyId, Inventory inventory, ITenantCrmDbContextFactory tenantFactory) =>
{
    await using var tenantDb = await tenantFactory.CreateAsync(companyId);
    tenantDb.Inventories.Add(inventory);
    await tenantDb.SaveChangesAsync();
    return Results.Ok(new { Message = "Inventory updated successfully!", InventoryId = inventory.InventoryId });
});

app.Run();

// ==========================================
// RECORDS MUST BE AT THE VERY BOTTOM
// ==========================================
public record CreateCompanyRequest(Company Company, CompanyDatabase CompanyDb);
public record CreateOrderRequest(int CustomerId, List<CreateOrderItemRequest> Items);
public record CreateOrderItemRequest(int ProductId, int Quantity);