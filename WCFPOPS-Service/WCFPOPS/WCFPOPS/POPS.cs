using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Data.SqlClient;
using System.ServiceModel;

namespace WCFPOPS
{
    public class POPS : IPOPS
    {
        SqlConnection sqlConnection;
        SqlCommand sqlCommand;

        //Create

        public void AddNewItem(Item value)
        {
            if (value.ItemCode.Length == 4 && value.ItemDescription.Length <= 15 && !string.IsNullOrEmpty(value.ItemDescription) && !string.IsNullOrEmpty(value.Price.ToString()))
            {
                using (sqlConnection = new SqlConnection())
                {
                    try
                    {
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "INSERT INTO ITEM VALUES(@ITCODE,@ITDESC,@ITRATE)";
                        sqlCommand.Parameters.AddWithValue("@ITCODE", value.ItemCode);
                        sqlCommand.Parameters.AddWithValue("@ITDESC", value.ItemDescription);
                        sqlCommand.Parameters.AddWithValue("@ITRATE", value.Price);
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    catch (SqlException sqlException)
                    {
                        if (sqlException.Number == 2627)
                        {
                            throw new FaultException<POException>(new POException("Duplicate Item Code"), "Item Already Exists with this Item Code");
                        }
                    }
                }
            }
            else
            {
                if (value.ItemDescription.Length > 15)
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Description Cannot Be Greater Than 15 characters");
                }
                else if (string.IsNullOrEmpty(value.ItemDescription))
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Description cannot be null");
                }
                else if (value.ItemCode.Length != 4)
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Code Cannot Be Greater Than 4 characters or Empty");
                }
                else if (value.Price == 0 || string.IsNullOrEmpty(value.Price.ToString()))
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Price cannot be Empty or 0");
                }
            }

        }

        public void AddNewSupplier(Supplier value)
        {
            if (value.SupplierNo.Length == 4 && value.SupplierName.Length <= 15 && !string.IsNullOrEmpty(value.SupplierName) && value.SupplierAddress.Length <= 40 && !string.IsNullOrEmpty(value.SupplierAddress))
            {
                using (sqlConnection = new SqlConnection())
                {
                    try
                    {
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "INSERT INTO SUPPLIER VALUES(@SUPLNO,@SUPLNAME,@SUPLADDR)";
                        sqlCommand.Parameters.AddWithValue("@SUPLNO", value.SupplierNo);
                        sqlCommand.Parameters.AddWithValue("@SUPLNAME", value.SupplierName);
                        sqlCommand.Parameters.AddWithValue("@SUPLADDR", value.SupplierAddress);
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();
                        sqlConnection.Close();
                    }
                    catch (SqlException sqlException)
                    {
                        if (sqlException.Number == 2627)
                        {
                            throw new FaultException<POException>(new POException("Duplicate Supplier Code"), "Already Supplier Exist with this Supplier Code");
                        }
                    }
                }
            }
            else
            {
                throw new FaultException<POException>(new POException("Failure! Supplier cannot be added. "), "Supplier Name and Supplier Address cannot greater the 15 and 40 resp.");
            }
        }

        public void MakeOrder(POMaster master, List<PODetail> poDetails)
        {
            using (TransactionScope scope = new TransactionScope())
            {
                using (sqlConnection = new SqlConnection())
                {
                    try
                    {
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "INSERT INTO POMASTER VALUES(@PurchaseNumber,@PurchaseDate,@SupplierNumber)";
                        sqlCommand.Parameters.AddWithValue("@PurchaseNumber", master.PurchaseNumber);
                        sqlCommand.Parameters.AddWithValue("@PurchaseDate", master.PurchaseDate);
                        sqlCommand.Parameters.AddWithValue("@SupplierNumber", master.SupplierNumber);
                        sqlConnection.Open();
                        sqlCommand.ExecuteNonQuery();

                        foreach (var order in poDetails)
                        {
                            sqlCommand.CommandText = "INSERT INTO PODETAIL VALUES('" + order.PurchaseNumber + "','" + order.ItemCode + "'," + order.Quantity + ")";
                            sqlCommand.ExecuteNonQuery();
                        }
                    }
                    catch (SqlException sqlException)
                    {
                        if (sqlException.Number == 2627)
                        {
                            throw new FaultException<POException>(new POException("Duplicate Order ID"), "Already Order Exist with this Order ID");
                        }
                    }
                }
                scope.Complete();
            }
        }

        public string GetOrderId()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "select max(PONO) as PONO from pomaster";
                sqlConnection.Open();
                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        dataReader.Read();
                        string po = (string)dataReader["PONO"];
                        int temp = Convert.ToInt16(po.Substring(1));
                        temp += 1;
                        po = null;
                        if (temp > 99)
                            po = "P" + temp;
                        else if (temp > 9)
                            po = "P0" + temp;
                        else
                            po = "P00" + temp;

                        sqlConnection.Close();
                        return po;
                    }
                    catch (InvalidOperationException)
                    {
                        return "P001";
                    }
                }
            }
        }

        public string GetItemId()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "select max(ITCODE) as ITCODE from ITEM";
                sqlConnection.Open();
                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        dataReader.Read();
                        string ic = (string)dataReader["ITCODE"];
                        int temp = Convert.ToInt16(ic.Substring(1));
                        temp += 1;
                        ic = null;
                        if (temp > 99)
                            ic = "I" + temp;
                        else if (temp > 9)
                            ic = "I0" + temp;
                        else
                            ic = "I00" + temp;

                        sqlConnection.Close();
                        return ic;
                    }
                    catch (InvalidOperationException)
                    {
                        return "I001";
                    }
                }
            }
        }

        public string GetSupplierId()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "select max(SUPLNO) as SUPLNO from SUPPLIER";
                sqlConnection.Open();
                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    try
                    {
                        dataReader.Read();
                        string sc = (string)dataReader["SUPLNO"];
                        int temp = Convert.ToInt16(sc.Substring(1));
                        temp += 1;
                        sc = null;
                        if (temp > 99)
                            sc = "S" + temp;
                        else if (temp > 9)
                            sc = "S0" + temp;
                        else
                            sc = "S00" + temp;
                        sqlConnection.Close();
                        return sc;
                    }
                    catch (InvalidOperationException)
                    {
                        return "S001";
                    }
                }
            }
        }

        //Read

        public List<Item> GetAllItems()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "SELECT * FROM ITEM";
                sqlConnection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                List<Item> itemList = new List<Item>();
                try
                {
                    while (dataReader.Read())
                    {
                        Item item = new Item();
                        item.ItemCode = (string)dataReader["ITCODE"];
                        item.ItemDescription = (string)dataReader["ITDESC"];
                        item.Price = (decimal)dataReader["ITRATE"];
                        itemList.Add(item);
                    }
                    return itemList;
                }
                catch (InvalidOperationException)
                {
                    throw new FaultException<POException>(new POException("No Item Found"), "There is no item in records.");
                }
            }
        }

        public Item GetItemByCode(string ItemCode)
        {
            if (ItemCode.Length == 4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT * FROM ITEM WHERE ITCODE = @ITCODE";
                    sqlCommand.Parameters.AddWithValue("@ITCODE", ItemCode);
                    sqlConnection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    Item item = new Item();
                    dataReader.Read();
                    try
                    {
                        item.ItemCode = (string)dataReader["ITCODE"];
                        item.ItemDescription = (string)dataReader["ITDESC"];
                        item.Price = (decimal)dataReader["ITRATE"];
                        return item;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException<POException>(new POException("Item Code Not Found"), "There is no such Item Code exist.");
                    }
                }
            }
            else
                throw new FaultException<POException>(new POException("Not A Valid Item Code"), "Item Code can only be of 4 characters.");
        }

        public List<Supplier> GetAllSuppliers()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "SELECT * FROM SUPPLIER";
                sqlConnection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                List<Supplier> supplierList = new List<Supplier>();
                try
                {
                    while (dataReader.Read())
                    {
                        Supplier supplier = new Supplier();
                        supplier.SupplierNo = (string)dataReader["SUPLNO"];
                        supplier.SupplierName = (string)dataReader["SUPLNAME"];
                        supplier.SupplierAddress = (string)dataReader["SUPLADDR"];
                        supplierList.Add(supplier);
                    }
                    return supplierList;
                }
                catch (InvalidOperationException)
                {
                    throw new FaultException<POException>(new POException("No Supplier Found"), "There is no supplier in records.");
                }
            }
        }

        public Supplier GetSupplierByCode(string SupplierCode)
        {
            if (SupplierCode.Length == 4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT * FROM SUPPLIER WHERE SUPLNO = @SUPLNO";
                    sqlCommand.Parameters.AddWithValue("@SUPLNO", SupplierCode);
                    sqlConnection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    Supplier supplier = new Supplier();
                    dataReader.Read();
                    try
                    {
                        supplier.SupplierNo = (string)dataReader["SUPLNO"];
                        supplier.SupplierName = (string)dataReader["SUPLNAME"];
                        supplier.SupplierAddress = (string)dataReader["SUPLADDR"];
                        return supplier;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException<POException>(new POException("Supplier Code Not Found"), "There is no such Supplier Code exist.");
                    }
                }
            }
            else
                throw new FaultException<POException>(new POException("Not A Valid Supplier Code"), "Supplier Code can only be of 4 characters.");
        }

        public List<PurchaseOrder> GetAllOrders()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "select mainResult.OrderNumber, mainResult.description as Description,mainResult.quantity as Quantity," +
                                             "supplier.SUPLNAME as SupplierName, mainResult.PODATE as OrderDate from supplier inner join" +
                                             "(select POMASTER.SUPLNO,POMASTER.PODATE,result.description,result.quantity,result.OrderNumber from POMASTER " +
                                             "inner join" +
                                             "(select item.ITDESC as description, PODETAIL.qty as Quantity, PODETAIL.PONO as OrderNumber from item " +
                                             "inner join podetail on " +
                                             "item.ITCODE = PODETAIL.itcode) as result " +
                                             "on POMASTER.PONO = result.OrderNumber ) as mainResult " +
                                             "on supplier.SUPLNO = mainResult.SUPLNO";
                sqlConnection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                List<PurchaseOrder> purchaseOrderList = new List<PurchaseOrder>();
                try
                {
                    while (dataReader.Read())
                    {
                        PurchaseOrder purchaseOrder = new PurchaseOrder();
                        purchaseOrder.OrderNumber = (string)dataReader["OrderNumber"];
                        purchaseOrder.Description = (string)dataReader["Description"];
                        purchaseOrder.Quantity = (int)dataReader["Quantity"];
                        purchaseOrder.SupplierName = (string)dataReader["SupplierName"];
                        purchaseOrder.OrderDate = (DateTime)dataReader["OrderDate"];

                        purchaseOrderList.Add(purchaseOrder);
                    }
                    if (purchaseOrderList.Count == 0)
                        throw new InvalidOperationException();
                    else
                        return purchaseOrderList;
                }
                catch (InvalidOperationException)
                {
                    throw new FaultException<POException>(new POException("No Order Found"), "There is no record of orders.");
                }
            }
        }

        public List<PurchaseOrderSpecial> GetAllOrdersSpecial()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "select mainResult.OrderNumber, mainResult.Description,mainResult.ItemCode ,mainResult.quantity as Quantity," +
                                            "supplier.SUPLNAME as SupplierName,supplier.SUPLNO as SupplierNumber, mainResult.PODATE as OrderDate from supplier inner join" +
                                            "(select POMASTER.SUPLNO,POMASTER.PODATE,result.description as Description,result.ItemCode,result.quantity,result.OrderNumber from POMASTER " +
                                            "inner join" +
                                            "(select item.ITDESC as description, item.ITCODE as ItemCode, PODETAIL.qty as Quantity, PODETAIL.PONO as OrderNumber from item " +
                                            "inner join podetail on " +
                                            "item.ITCODE = PODETAIL.itcode) as result " +
                                            "on POMASTER.PONO = result.OrderNumber ) as mainResult " +
                                            "on supplier.SUPLNO = mainResult.SUPLNO ";
                sqlConnection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                List<PurchaseOrderSpecial> purchaseOrderList = new List<PurchaseOrderSpecial>();
                try
                {
                    while (dataReader.Read())
                    {
                        PurchaseOrderSpecial purchaseOrder = new PurchaseOrderSpecial();
                        purchaseOrder.OrderNumber = (string)dataReader["OrderNumber"];
                        purchaseOrder.Description = (string)dataReader["Description"];
                        purchaseOrder.ItemCode = (string)dataReader["ItemCode"];
                        purchaseOrder.Quantity = (int)dataReader["Quantity"];
                        purchaseOrder.SupplierName = (string)dataReader["SupplierName"];
                        purchaseOrder.SupplierCode = (string)dataReader["SupplierNumber"];
                        purchaseOrder.OrderDate = (DateTime)dataReader["OrderDate"];

                        purchaseOrderList.Add(purchaseOrder);
                    }
                    if (purchaseOrderList.Count == 0)
                        throw new InvalidOperationException();
                    else
                        return purchaseOrderList;
                }
                catch (InvalidOperationException)
                {
                    throw new FaultException<POException>(new POException("No Order Found"), "There is no record of orders.");
                }
            }
        }

        public List<PurchaseOrderSpecial> GetAllOrdersSpecialByID(string OrderID)
        {
            if (OrderID.Length == 4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "select mainResult.OrderNumber, mainResult.Description,mainResult.ItemCode ,mainResult.quantity as Quantity," +
                                                "supplier.SUPLNAME as SupplierName,supplier.SUPLNO as SupplierNumber, mainResult.PODATE as OrderDate from supplier inner join" +
                                                "(select POMASTER.SUPLNO,POMASTER.PODATE,result.description as Description,result.ItemCode,result.quantity,result.OrderNumber from POMASTER " +
                                                "inner join" +
                                                "(select item.ITDESC as description, item.ITCODE as ItemCode, PODETAIL.qty as Quantity, PODETAIL.PONO as OrderNumber from item " +
                                                "inner join podetail on " +
                                                "item.ITCODE = PODETAIL.itcode) as result " +
                                                "on POMASTER.PONO = result.OrderNumber ) as mainResult " +
                                                "on supplier.SUPLNO = mainResult.SUPLNO " +
                                                "where OrderNumber=@OrderNumber";
                    sqlCommand.Parameters.AddWithValue("@OrderNumber", OrderID);
                    sqlConnection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    List<PurchaseOrderSpecial> purchaseOrderList = new List<PurchaseOrderSpecial>();
                    try
                    {
                        while (dataReader.Read())
                        {
                            PurchaseOrderSpecial purchaseOrder = new PurchaseOrderSpecial();
                            purchaseOrder.OrderNumber = (string)dataReader["OrderNumber"];
                            purchaseOrder.Description = (string)dataReader["Description"];
                            purchaseOrder.ItemCode = (string)dataReader["ItemCode"];
                            purchaseOrder.Quantity = (int)dataReader["Quantity"];
                            purchaseOrder.SupplierName = (string)dataReader["SupplierName"];
                            purchaseOrder.SupplierCode = (string)dataReader["SupplierNumber"];
                            purchaseOrder.OrderDate = (DateTime)dataReader["OrderDate"];

                            purchaseOrderList.Add(purchaseOrder);
                        }
                        if (purchaseOrderList.Count == 0)
                            throw new InvalidOperationException();
                        else
                            return purchaseOrderList;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException<POException>(new POException("No Order Found"), "There is no record of orders.");
                    }
                }
            }
            else
                throw new FaultException<POException>(new POException("Not A Valid Order ID"), "Order ID can only be of 4 characters.");
        }

        public List<PurchaseOrder> GetOrderByOrderId(string orderId)
        {
            if (orderId.Length == 4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "select mainResult.OrderNumber, mainResult.description as Description,mainResult.quantity as Quantity," +
                                                 "supplier.SUPLNAME as SupplierName, mainResult.PODATE as OrderDate from supplier inner join" +
                                                 "(select POMASTER.SUPLNO,POMASTER.PODATE,result.description,result.quantity,result.OrderNumber from POMASTER " +
                                                 "inner join" +
                                                 "(select item.ITDESC as description, PODETAIL.qty as Quantity, PODETAIL.PONO as OrderNumber from item " +
                                                 "inner join podetail on " +
                                                 "item.ITCODE = PODETAIL.itcode) as result " +
                                                 "on POMASTER.PONO = result.OrderNumber ) as mainResult " +
                                                 "on supplier.SUPLNO = mainResult.SUPLNO " +
                                                 "where OrderNumber=@OrderNumber";
                    sqlCommand.Parameters.AddWithValue("@OrderNumber", orderId);
                    sqlConnection.Open();
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    List<PurchaseOrder> purchaseOrderList = new List<PurchaseOrder>();
                    try
                    {
                        while (dataReader.Read())
                        {
                            PurchaseOrder purchaseOrder = new PurchaseOrder();
                            purchaseOrder.OrderNumber = (string)dataReader["OrderNumber"];
                            purchaseOrder.Description = (string)dataReader["Description"];
                            purchaseOrder.Quantity = (int)dataReader["Quantity"];
                            purchaseOrder.SupplierName = (string)dataReader["SupplierName"];
                            purchaseOrder.OrderDate = (DateTime)dataReader["OrderDate"];

                            purchaseOrderList.Add(purchaseOrder);
                        }
                        if (purchaseOrderList.Count == 0)
                            throw new InvalidOperationException();
                        else
                            return purchaseOrderList;
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException<POException>(new POException("No Order Found"), "Please verify the OrderID.");
                    }
                }
            }
            else
                throw new FaultException<POException>(new POException("Invalid OrderId"), "Order Id should be of 4 characters.");
        }

        public List<POMaster> GetPoMaster()
        {
            using (sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                sqlCommand = new SqlCommand();
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandText = "SELECT * FROM POMASTER";
                sqlConnection.Open();
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                List<POMaster> poMasterList = new List<POMaster>();
                while (dataReader.Read())
                {
                    POMaster poMaster = new POMaster();
                    poMaster.PurchaseNumber = (string)dataReader["PONO"];
                    poMaster.PurchaseDate = (DateTime)dataReader["PODATE"];
                    poMaster.SupplierNumber = (string)dataReader["SUPLNO"];

                    poMasterList.Add(poMaster);
                }
                if (poMasterList.Count != 0)
                {
                    return poMasterList;
                }
                else
                    throw new FaultException<POException>(new POException("No Record Found"), "");
            }
        }

        //Update

        public void UpdateItem(Item item)
        {
            if (item.ItemCode.Length == 4 && item.ItemDescription.Length <= 15 && !string.IsNullOrEmpty(item.ItemDescription) && !string.IsNullOrEmpty(item.Price.ToString()))
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT COUNT(ITCODE) as IsItemExist FROM ITEM WHERE ITCODE=@ITCODE";
                    sqlCommand.Parameters.AddWithValue("@ITCODE", item.ItemCode);
                    sqlConnection.Open();
                    SqlDataReader dataReader;
                    using (dataReader = sqlCommand.ExecuteReader())
                    {
                        dataReader.Read();
                        if (Convert.ToInt16(dataReader["IsItemExist"]) == 1)
                        {
                            dataReader.Close();
                            sqlCommand.CommandText = "UPDATE ITEM SET ITDESC=@ITDESC , ITRATE=@ITRATE WHERE ITCODE=@ITCODE";
                            sqlCommand.Parameters.AddWithValue("@ITDESC", item.ItemDescription);
                            sqlCommand.Parameters.AddWithValue("@ITRATE", item.Price);
                            sqlCommand.ExecuteNonQuery();
                        }
                        else
                            throw new FaultException<POException>(new POException("Item Does Not Exist"), "");
                    }
                }
            }
            else
            {
                if (item.ItemDescription.Length > 15)
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Description Cannot Be Greater Than 15 characters");
                }
                else if (string.IsNullOrEmpty(item.ItemDescription))
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Description cannot be null");
                }
                else if (item.ItemCode.Length != 4)
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Item Code Cannot Be Greater Than 4 characters or Empty");
                }
                else if (item.Price == 0 || string.IsNullOrEmpty(item.Price.ToString()))
                {
                    throw new FaultException<POException>(new POException("Failure! Item cannot be added. "), "Price cannot be Empty or 0");
                }

            }
        }

        public void UpdateSupplier(Supplier supplier)
        {
            if (supplier.SupplierNo.Length == 4 && supplier.SupplierName.Length <= 15 && !string.IsNullOrEmpty(supplier.SupplierName) && supplier.SupplierAddress.Length <= 40 && !string.IsNullOrEmpty(supplier.SupplierAddress))
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT COUNT(SUPLNO) AS IsSupplierExist FROM SUPPLIER WHERE SUPLNO=@SUPLNO";
                    sqlCommand.Parameters.AddWithValue("@SUPLNO", supplier.SupplierNo);
                    sqlConnection.Open();
                    SqlDataReader dataReader;
                    using (dataReader = sqlCommand.ExecuteReader())
                    {
                        dataReader.Read();
                        if (Convert.ToInt16(dataReader["IsSupplierExist"]) == 1)
                        {
                            dataReader.Close();
                            sqlCommand.CommandText = "UPDATE SUPPLIER SET SUPLNAME=@SUPLNAME , SUPLADDR=@SUPLADDR WHERE SUPLNO=@SUPLNO";
                            sqlCommand.Parameters.AddWithValue("@SUPLNAME", supplier.SupplierName);
                            sqlCommand.Parameters.AddWithValue("@SUPLADDR", supplier.SupplierAddress);
                            sqlCommand.ExecuteNonQuery();
                        }
                        else
                            throw new FaultException<POException>(new POException("Item Does Not Exist"), "");
                    }
                }
            }
            else
            {
                throw new FaultException<POException>(new POException("Failure! Supplier cannot be added. "), "Supplier Name and Supplier Address cannot greater the 15 and 40.");
            }
        }

        public void UpdateOrderSupplier(POMaster pomaster)
        {
            if (pomaster.PurchaseNumber.Length == 4 && pomaster.SupplierNumber.Length == 4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlCommand.CommandText = "SELECT COUNT(PONO) AS IsOrderExist FROM POMASTER WHERE PONO=@PONO";
                    sqlCommand.Parameters.AddWithValue("@PONO", pomaster.PurchaseNumber);
                    sqlConnection.Open();
                    SqlDataReader dataReader;
                    using (dataReader = sqlCommand.ExecuteReader())
                    {
                        dataReader.Read();
                        if (Convert.ToInt16(dataReader["IsOrderExist"]) == 1)
                        {
                            dataReader.Close();
                            sqlCommand.CommandText = "UPDATE POMASTER SET SUPLNO = @SUPLNO WHERE PONO = @PONO";
                            sqlCommand.Parameters.AddWithValue("@SUPLNO", pomaster.SupplierNumber);
                            sqlCommand.ExecuteNonQuery();
                        }
                        else
                            throw new FaultException<POException>(new POException("Order Does Not Exist"), "");
                    }
                }
            }
            else
            {
                throw new FaultException<POException>(new POException("Not a valid Order Id or Supplier Code"), "");
            }
        }

        public void UpdateOrderQuantity(PODetail poDetail)
        {
            if (poDetail.ItemCode.Length==4 && poDetail.PurchaseNumber.Length==4)
            {
                using (sqlConnection = new SqlConnection())
                {
                    sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                    sqlCommand = new SqlCommand();
                    sqlCommand.Connection = sqlConnection;
                    sqlConnection.Open();
                    try
                    {
                        sqlCommand.CommandText = $"UPDATE PODETAIL SET QTY='{poDetail.Quantity}' WHERE PONO='{poDetail.PurchaseNumber}'AND ITCODE='{poDetail.ItemCode}'";
                        sqlCommand.ExecuteNonQuery();
                    }
                    catch (InvalidOperationException)
                    {
                        throw new FaultException<POException>(new POException("Update Failed"), "");
                    }
                }
            }
            else
                throw new FaultException<POException>(new POException("Not a valid OrderId or ItemCode"), "");
        }


        //Delete

        public void DeleteItem(string ItemCode)
        {
            if (ItemCode.Length == 4)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (sqlConnection = new SqlConnection())
                    {
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT COUNT(ITCODE) as ItemExist FROM ITEM WHERE ITCODE=@ITCODE";
                        sqlCommand.Parameters.AddWithValue("@ITCODE", ItemCode);
                        sqlConnection.Open();
                        SqlDataReader dataReader;
                        using (dataReader = sqlCommand.ExecuteReader())
                        {
                            dataReader.Read();
                            if (Convert.ToInt16(dataReader["ItemExist"]) == 1)
                            {
                                sqlCommand.CommandText = "SELECT COUNT(PONO) as OrderCount FROM PODETAIL WHERE ITCODE=@ITCODE";
                                dataReader.Close();
                                dataReader = sqlCommand.ExecuteReader();
                                dataReader.Read();
                                if (Convert.ToInt16(dataReader["OrderCount"]) == 0)
                                {
                                    dataReader.Close();
                                    sqlCommand.CommandText = "DELETE FROM ITEM WHERE ITCODE=@ITCODE";
                                    sqlCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    List<string> ponoList = new List<string>();
                                    dataReader.Close();
                                    sqlCommand.CommandText = "SELECT PONO FROM PODETAIL WHERE ITCODE=@ITCODE";
                                    dataReader = sqlCommand.ExecuteReader();
                                    while (dataReader.Read())
                                    {
                                        string pono = (string)dataReader["PONO"];
                                        ponoList.Add(pono);
                                    }
                                    dataReader.Close();
                                    sqlCommand.CommandText = "DELETE FROM PODETAIL WHERE ITCODE=@ITCODE";
                                    sqlCommand.ExecuteNonQuery();

                                    sqlCommand.CommandText = "DELETE FROM ITEM WHERE ITCODE=@ITCODE";
                                    sqlCommand.ExecuteNonQuery();
                                    foreach (var pono in ponoList)
                                    {
                                        sqlCommand.CommandText = "SELECT COUNT(PONO) as PonoExist FROM PODETAIL WHERE PONO ='" + pono + "'";
                                        dataReader = sqlCommand.ExecuteReader();
                                        dataReader.Read();
                                        if (Convert.ToInt16(dataReader["PonoExist"]) == 0)
                                        {
                                            dataReader.Close();
                                            sqlCommand.CommandText = "DELETE FROM POMASTER WHERE PONO='" + pono + "'";
                                            sqlCommand.ExecuteNonQuery();
                                        }
                                        else
                                            dataReader.Close();
                                    }
                                }
                            }
                            else
                                throw new FaultException<POException>(new POException("Item Does Not Exist"), "");
                        }
                    }
                    scope.Complete();
                }
            }
            else
                throw new FaultException<POException>(new POException("Invalid Item Code"), "");
        }

        public void DeleteSupplier(string SupplierCode)
        {
            if (SupplierCode.Length == 4)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (sqlConnection = new SqlConnection())
                    {
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand = new SqlCommand();
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT COUNT(SUPLNO) AS IsSupplierExist FROM SUPPLIER WHERE SUPLNO=@SUPLNO";
                        sqlCommand.Parameters.AddWithValue("@SUPLNO", SupplierCode);
                        sqlConnection.Open();
                        SqlDataReader dataReader;
                        using (dataReader = sqlCommand.ExecuteReader())
                        {
                            dataReader.Read();
                            if (Convert.ToInt16(dataReader["IsSupplierExist"]) == 1)
                            {
                                dataReader.Close();
                                sqlCommand.CommandText = "SELECT COUNT(PONO) AS IsOrderExist FROM POMASTER WHERE SUPLNO=@SUPLNO";
                                dataReader = sqlCommand.ExecuteReader();
                                dataReader.Read();
                                if (Convert.ToInt16(dataReader["IsOrderExist"]) == 0)
                                {
                                    dataReader.Close();
                                    sqlCommand.CommandText = "DELETE FROM SUPPLIER WHERE SUPLNO=@SUPLNO";
                                    sqlCommand.ExecuteNonQuery();
                                }
                                else
                                {
                                    dataReader.Close();
                                    List<string> ponoList = new List<string>();
                                    sqlCommand.CommandText = "SELECT PONO FROM POMASTER WHERE SUPLNO=@SUPLNO";
                                    dataReader = sqlCommand.ExecuteReader();
                                    while (dataReader.Read())
                                    {
                                        ponoList.Add(Convert.ToString(dataReader["PONO"]));
                                    }
                                    dataReader.Close();
                                    foreach (var pono in ponoList)
                                    {
                                        sqlCommand.CommandText = $"DELETE FROM PODETAIL WHERE PONO='{pono}'";
                                        sqlCommand.ExecuteNonQuery();
                                    }
                                    sqlCommand.CommandText = "DELETE FROM POMASTER WHERE SUPLNO=@SUPLNO";
                                }
                            }
                            else
                                throw new FaultException<POException>(new POException("Supplier Code Does Not Exist."), "");
                        }
                    }
                    scope.Complete();
                }
            }
            else
                throw new FaultException<POException>(new POException("Invalid Supplier Code"), "");
        }

        public void DeleteOrder(string OrderId)
        {
            if (OrderId.Length == 4)
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    using (sqlConnection = new SqlConnection())
                    {
                        sqlCommand = new SqlCommand();
                        sqlConnection.ConnectionString = "Data Source=PAL-LEGION; Database=PODb; integrated security=true";
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.CommandText = "SELECT COUNT(PONO) AS IsOrderExist FROM POMASTER WHERE PONO=@PONO";
                        sqlCommand.Parameters.AddWithValue("@PONO", OrderId);
                        sqlConnection.Open();
                        SqlDataReader dataReader;
                        using (dataReader = sqlCommand.ExecuteReader())
                        {
                            dataReader.Read();
                            if (Convert.ToInt16(dataReader["IsOrderExist"]) == 1)
                            {
                                dataReader.Close();
                                sqlCommand.CommandText = "DELETE FROM PODETAIL WHERE PONO=@PONO";
                                sqlCommand.ExecuteNonQuery();

                                sqlCommand.CommandText = "DELETE FROM POMASTER WHERE PONO=@PONO";
                                sqlCommand.ExecuteNonQuery();
                            }
                            else
                                throw new FaultException<POException>(new POException("Order Does Not Exist."), "");
                        }
                    }
                    scope.Complete();
                }
            }
            else
                throw new FaultException<POException>(new POException("Invalid Order ID"), "");
        }

    }
}
