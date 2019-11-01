using OngoingREST;
using System;
using System.Collections.Generic;
using System.Text;

namespace OngoingREST_Example
{
   class Program
   {
      static void Main(string[] args)
      {
         // Ongoing WMS is a Warehouse Management System based in Sweden.
         // This file demonstrates one way of integrating with the WMS' REST API.
         // For more information, please see:
         // https://developer.ongoingwarehouse.com/REST/v1/index.html
         // https://www.ongoingwarehouse.com/

         // These are the credentials and other information which are required to connect to the API.
         // Ask the warehouse to generate them for you - https://docs.ongoingwarehouse.com/Manuals/API-Access
         var userName = "WSI...";
         var password = "...";
         var baseUrl = "https://api.ongoingsystems.se/apidemo/";
         var goodsOwnerId = 162;

         // Set up all the client objects.
         var client = new System.Net.Http.HttpClient();
         client.DefaultRequestHeaders.Add("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userName}:{password}")));
         var articlesClient = new ArticlesClient(client) { BaseUrl = baseUrl };
         var ordersClient = new OrdersClient(client) { BaseUrl = baseUrl };
         var purchaseOrderClient = new PurchaseOrdersClient(client) { BaseUrl = baseUrl };
         var inventoryAdjustmentsClient = new InventoryAdjustmentsClient(client) { BaseUrl = baseUrl };
         var transporterContractsClient = new TransporterContractsClient(client) { BaseUrl = baseUrl };

         // Run the tests.
         TestArticles(goodsOwnerId, articlesClient);
         TestOrders(goodsOwnerId, ordersClient);
         TestPurchaseOrders(goodsOwnerId, purchaseOrderClient);
         TestInventoryAdjustments(goodsOwnerId, inventoryAdjustmentsClient);
         TestTransporterContracts(goodsOwnerId, transporterContractsClient);

         Console.WriteLine("Press Enter to exit.");

         Console.Read();
      }

      private static void TestArticles(int goodsOwnerId, ArticlesClient articlesClient)
      {
         Console.WriteLine("Running article examples...");
         Console.WriteLine("");

         // Define a new article.
         var article = new PostArticleModel()
         {
            GoodsOwnerId = goodsOwnerId,
            ArticleNumber = "12345",
            ArticleName = "Test article",
            Weight = 1.7m, // Unit is kilograms.
            BarCodeInfo = new PostArticleBarCodeInfo()
            {
               BarCode = "733123"
            }
         };

         // Send the new article to the warehouse, thus creating it.
         var createResponse = articlesClient.Put3(article);

         // Update the barcode to something else.
         article.BarCodeInfo.BarCode = "507543";
         var updateResponse = articlesClient.Put3(article);

         // You may query for articles using various ways. The response will include various article data (including stock balances).
         var getArticleByArticleSystemId = articlesClient.Get(createResponse.ArticleSystemId.Value);
         var getArticleByArticleNumber = articlesClient.GetAll(goodsOwnerId, "12345", null, null);

         // If you have a file (such as an image), you may attach it to the article.
         var imagePath = @"C:\WMS\12345.png";
         if (System.IO.File.Exists(imagePath))
         {
            var fileBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(imagePath));
            var file = new PostFileModel() { FileName = "12345.png", FileDataBase64 = fileBase64, MimeType = "image/png" };
            articlesClient.Post(createResponse.ArticleSystemId.Value, file);
         }
      }

      private static void TestOrders(int goodsOwnerId, OrdersClient ordersClient)
      {
         Console.WriteLine("Running order examples...");
         Console.WriteLine("");

         var orderNumber = "PR78912"; // A unique order number.

         // Define a new order.
         var order = new PostOrderModel()
         {
            GoodsOwnerId = goodsOwnerId,
            Consignee = new PostOrderConsignee()
            {
               Name = "Test Testsson",
               Address1 = "Test Street 1",
               City = "Test City",
               PostCode = "111111",
               CountryCode = "SE"
            },
            OrderNumber = orderNumber,
            OrderLines = new List<PostOrderLine>()
            {
               new PostOrderLine() { RowNumber = "OrderRow1", ArticleNumber = "1234", NumberOfItems = 2 }, // Each order line must have a unique row number.
               new PostOrderLine() { RowNumber = "OrderRow2", ArticleNumber = "7879", NumberOfItems = 4 },
            }
         };

         // Send the new order to the warehouse, thus creating it.
         var createResponse = ordersClient.PutOrder(order);

         // If we send in the same order number again, the system will try to update the order.
         // It is possible to update an order as long as the warehouse has not started working on it.
         order.OrderLines.Add(new PostOrderLine() { RowNumber = "OrderRow3", ArticleNumber = "5943", NumberOfItems = 9 });
         var updateResponse = ordersClient.PutOrder(order);

         // You may upload files (documents, labels, etc) to orders.
         var pdfPath = @"C:\WMS\Invoice-PR78912.pdf";
         if (System.IO.File.Exists(pdfPath))
         {
            var fileBase64 = Convert.ToBase64String(System.IO.File.ReadAllBytes(pdfPath));
            var file = new PostFileModel() { FileName = "Invoice-PR78912.pdf", FileDataBase64 = fileBase64, MimeType = "application/pdf" };
            ordersClient.Post(createResponse.OrderId.Value, file);
         }

         // You may query for orders using various ways.
         // For instance using orderId:
         var getOrderByOrderId = ordersClient.Get(createResponse.OrderId.Value);

         // Or by order number:
         var getOrderByOrderNumber = ordersClient.GetAll(goodsOwnerId, orderNumber, null, null, null, null, null, null, null);

         // Or get all orders which have been shipped during a particular period:
         var from = new DateTimeOffset(new DateTime(2019, 11, 1, 12, 0, 0));
         var to = new DateTimeOffset(new DateTime(2019, 11, 1, 13, 0, 0));
         var getOrdersByShippedTime = ordersClient.GetAll(goodsOwnerId, null, from, to, null, null, null, null, null);
         foreach (var shippedOrder in getOrdersByShippedTime)
         {
            // Handle "shippedOrder" somehow.
         }
      }

      private static void TestPurchaseOrders(int goodsOwnerId, PurchaseOrdersClient purchaseOrdersClient)
      {
         Console.WriteLine("Running purchase order examples...");
         Console.WriteLine("");

         var purchaseOrderNumber = "PO123"; // A unique number for the purchase order.

         // Define a new purchase order.
         var purchaseOrder = new PostPurchaseOrderModel()
         {
            GoodsOwnerId = goodsOwnerId,
            PurchaseOrderNumber = purchaseOrderNumber,
            InDate = new DateTimeOffset(new DateTime(2019, 11, 1)), // Expected in date.
            PurchaseOrderLines = new List<PostPurchaseOrderLine>()
             {
                new PostPurchaseOrderLine() { RowNumber = "PORow1", ArticleNumber = "1234", NumberOfItems = 10},// Each purchase order line must have a unique row number.
                new PostPurchaseOrderLine() { RowNumber = "PORow2", ArticleNumber = "7897", NumberOfItems = 25},
             }
         };

         // Send the new purchase order to the warehouse, thus creating it.
         var createResponse = purchaseOrdersClient.Put2(purchaseOrder);

         // If we send in the same purchase order number again, the system will try to update the order.
         // It is possible to update a purchase order as long as the warehouse has not started working on it.
         purchaseOrder.PurchaseOrderLines.Add(new PostPurchaseOrderLine() { RowNumber = "PORow3", ArticleNumber = "5943", NumberOfItems = 17 });
         var updateResponse = purchaseOrdersClient.Put2(purchaseOrder);

         // You may query for purchase orders using various ways.
         // For instance using purchaseOrderId:
         var getPurchaseOrderByPurchaseOrderId = purchaseOrdersClient.Get(createResponse.PurchaseOrderId.Value);

         // Or by purchase order number:
         var getPurchaseOrderByPurchaseOrderNumber = purchaseOrdersClient.GetAll(goodsOwnerId, purchaseOrderNumber, null, null, null, null, null, null, null);

         // Or get all orders which have been received during a period:
         var from = new DateTimeOffset(new DateTime(2019, 11, 1, 12, 0, 0));
         var to = new DateTimeOffset(new DateTime(2019, 11, 1, 13, 0, 0));
         var getPurchaseOrdersByReceiveTime = purchaseOrdersClient.GetAll(goodsOwnerId, null, from, to, null, null, null, null, null);
         foreach (var receivedPurchaseOrder in getPurchaseOrdersByReceiveTime)
         {
            // Handle "receivedPurchaseOrder" somehow.
         }
      }

      private static void TestInventoryAdjustments(int goodsOwnerId, InventoryAdjustmentsClient inventoryAdjustmentsClient)
      {
         Console.WriteLine("Running inventory adjustment examples...");
         Console.WriteLine("");

         // If the warehouse makes a mistake and needs to correct it (e.g. an item is dropped and needs to be scrapped),
         // the resulting transaction is called an "inventory transaction".
         // You can query for all transactions made during a certain period:
         var from = new DateTimeOffset(new DateTime(2019, 11, 1, 11, 0, 0));
         var to = new DateTimeOffset(new DateTime(2019, 11, 1, 12, 0, 0));
         var getInventoryAdjustments = inventoryAdjustmentsClient.GetAll(goodsOwnerId, from, to);
      }

      private static void TestTransporterContracts(int goodsOwnerId, TransporterContractsClient transporterContractsClient)
      {
         // Transporter contracts are the available shipping methods.
         // When you create orders, you may send in a shipping method if you wish, so that warehouse knows exactly
         // which transporter to use when shipping the order.
         var contracts = transporterContractsClient.Get(goodsOwnerId);
         Console.WriteLine("Available transporter contracts:");
         Console.WriteLine("");
         foreach (var contract in contracts)
         {
            Console.WriteLine($"Transporter '{contract.TransporterName}', transporter code '{contract.TransporterCode}'");
            foreach (var service in contract.TransporterServices)
            {
               Console.WriteLine($"- Service '{service.TransporterServiceName}', service code '{service.TransporterServiceCode}'");
            }
            Console.WriteLine("");
         }
      }
   }
}