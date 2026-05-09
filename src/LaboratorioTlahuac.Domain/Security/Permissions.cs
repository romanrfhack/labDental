namespace LaboratorioTlahuac.Domain.Security;

public static class Permissions
{
    public const string OrdersView = "orders.view";
    public const string OrdersCreate = "orders.create";
    public const string OrdersEdit = "orders.edit";
    public const string OrdersDelete = "orders.delete";
    public const string OrdersChangeStatus = "orders.changeStatus";
    public const string PaymentsView = "payments.view";
    public const string PaymentsCreate = "payments.create";
    public const string PaymentsCancel = "payments.cancel";
    public const string CustomersView = "customers.view";
    public const string CustomersCreate = "customers.create";
    public const string CustomersEdit = "customers.edit";
    public const string InventoryView = "inventory.view";
    public const string InventoryCreate = "inventory.create";
    public const string InventoryAdjust = "inventory.adjust";
    public const string SuppliersView = "suppliers.view";
    public const string SuppliersCreate = "suppliers.create";
    public const string UsersManage = "users.manage";
    public const string RolesManage = "roles.manage";
    public const string ReportsView = "reports.view";

    public static IReadOnlyDictionary<string, string> Descriptions { get; } =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [OrdersView] = "Ver ordenes de trabajo.",
            [OrdersCreate] = "Crear ordenes de trabajo.",
            [OrdersEdit] = "Editar ordenes de trabajo.",
            [OrdersDelete] = "Eliminar ordenes de trabajo.",
            [OrdersChangeStatus] = "Cambiar estado de ordenes.",
            [PaymentsView] = "Ver pagos y saldos.",
            [PaymentsCreate] = "Registrar pagos.",
            [PaymentsCancel] = "Cancelar pagos.",
            [CustomersView] = "Ver clientes, doctores y clinicas.",
            [CustomersCreate] = "Crear clientes, doctores y clinicas.",
            [CustomersEdit] = "Editar clientes, doctores y clinicas.",
            [InventoryView] = "Ver inventario.",
            [InventoryCreate] = "Crear materiales de inventario.",
            [InventoryAdjust] = "Ajustar existencias de inventario.",
            [SuppliersView] = "Ver proveedores.",
            [SuppliersCreate] = "Crear proveedores.",
            [UsersManage] = "Administrar usuarios.",
            [RolesManage] = "Administrar roles y permisos.",
            [ReportsView] = "Ver reportes y dashboard."
        };

    public static IReadOnlySet<string> All { get; } = new HashSet<string>(StringComparer.Ordinal)
    {
        OrdersView,
        OrdersCreate,
        OrdersEdit,
        OrdersDelete,
        OrdersChangeStatus,
        PaymentsView,
        PaymentsCreate,
        PaymentsCancel,
        CustomersView,
        CustomersCreate,
        CustomersEdit,
        InventoryView,
        InventoryCreate,
        InventoryAdjust,
        SuppliersView,
        SuppliersCreate,
        UsersManage,
        RolesManage,
        ReportsView
    };
}
