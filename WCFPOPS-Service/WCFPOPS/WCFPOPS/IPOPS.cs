using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace WCFPOPS
{
    [ServiceContract]
    public interface IPOPS
    {
        //Create

        [OperationContract]
        [FaultContract(typeof(POException))]
        void AddNewItem(Item value);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void AddNewSupplier(Supplier value);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void MakeOrder(POMaster master, List<PODetail> poDetails);

        [OperationContract]
        string GetOrderId();

        [OperationContract]
        string GetItemId();

        [OperationContract]
        string GetSupplierId();

        //Read

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<Item> GetAllItems();

        [OperationContract]
        [FaultContract(typeof(POException))]
        Item GetItemByCode(string ItemCode);

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<Supplier> GetAllSuppliers();

        [OperationContract]
        [FaultContract(typeof(POException))]
        Supplier GetSupplierByCode(string SupplierCode);

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<PurchaseOrder> GetAllOrders();

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<PurchaseOrderSpecial> GetAllOrdersSpecial();

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<PurchaseOrderSpecial> GetAllOrdersSpecialByID(string OrderID);

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<PurchaseOrder> GetOrderByOrderId(string orderId);

        [OperationContract]
        [FaultContract(typeof(POException))]
        List<POMaster> GetPoMaster();

        //Update

        [OperationContract]
        [FaultContract(typeof(POException))]
        void UpdateItem(Item item);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void UpdateSupplier(Supplier supplier);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void UpdateOrderSupplier(POMaster poMaster);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void UpdateOrderQuantity(PODetail poDetail);

        //Delete

        [OperationContract]
        [FaultContract(typeof(POException))]
        void DeleteItem(string ItemCode);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void DeleteSupplier(string SupplierCode);

        [OperationContract]
        [FaultContract(typeof(POException))]
        void DeleteOrder(string OrderId);
    }
}
