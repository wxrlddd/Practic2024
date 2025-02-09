using System;
using System.Collections.Generic;
using System.IO;
using static CodeSmellsExample.Logger;

namespace CodeSmellsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config("orders.txt", "logs.txt");
            var logger = new Logger(config);
            var priceService = new PriceService();
            var discountService = new DiscountService();
            var orderPersistenceService = new OrderPersistenceService(config.OrderFilePath);
            var orderService = new OrderService(priceService, discountService, orderPersistenceService);
            var processor = new OrderProcessor(orderService, logger);

            // Створюємо об'єкти Order
            var order1 = new Order("John Doe", 5, CustomerType.Standard);
            var order2 = new Order("Jane Doe", 15, CustomerType.Premium);
            var order3 = new Order("Alice", 2, CustomerType.Standard);

            processor.ProcessOrder(order1);
            processor.ProcessOrder(order2);
            processor.ProcessOrder(order3);

            logger.PrintLogs();
        }
    }

    class Config
    {
        public string OrderFilePath { get; }
        public string LogFilePath { get; }

        public Config(string orderFilePath, string logFilePath)
        {
            OrderFilePath = orderFilePath;
            LogFilePath = logFilePath;
        }
    }

    class OrderProcessor
    {
        private readonly OrderService _orderService;
        private readonly Logger _logger;

        public OrderProcessor(OrderService orderService, Logger logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        public void ProcessOrder(Order order)
        {
            double finalPrice = _orderService.HandleOrder(order); 
            LogOrderProcessing(order, finalPrice);
            PrintOrderResult(order, finalPrice);
        }

        private void PrintOrderResult(Order order, double finalPrice)
        {
            Console.WriteLine($"Customer: {order.CustomerName}, Quantity: {order.Quantity}, Total Price: {finalPrice}");
        }

        private void LogOrderProcessing(Order order, double finalPrice)
        {
            _logger.Log($"Processed order for {order.CustomerName}, quantity: {order.Quantity}, price: {finalPrice}, type: {order.CustomerType}");
        }
    }

    class OrderService
    {
        private readonly PriceService _priceService;
        private readonly DiscountService _discountService;
        private readonly OrderPersistenceService _orderPersistenceService;

        public OrderService(PriceService priceService, DiscountService discountService, OrderPersistenceService orderPersistenceService)
        {
            _priceService = priceService;
            _discountService = discountService;
            _orderPersistenceService = orderPersistenceService;
        }

        public double HandleOrder(Order order)
        {
            double price = _priceService.CalculatePrice(order.Quantity);
            price = _discountService.ApplyDiscount(price, order.CustomerType);
            _orderPersistenceService.SaveOrder(order.CustomerName, order.Quantity, price);
            return price;
        }
    }

    class PriceService
    {
        public double CalculatePrice(int quantity)
        {
            double price = quantity * 10;
            if (quantity > 10)
            {
                price *= 0.9; // Знижка для кількості більше 10
            }
            return price;
        }
    }

    class DiscountService
    {
        public double ApplyDiscount(double price, CustomerType customerType)
        {
            if (customerType == CustomerType.Premium)
            {
                return price * 0.85; // Знижка для преміум клієнтів
            }
            return price;
        }
    }

    class OrderPersistenceService
    {
        private readonly string _filePath;

        public OrderPersistenceService(string filePath)
        {
            _filePath = filePath;
        }

        public void SaveOrder(string customer, int quantity, double price)
        {
            File.AppendAllText(_filePath, $"{customer}, {quantity}, {price}\n");
        }
    }

    class Logger
    {
        private readonly string _logFilePath;
        private List<string> logs = new List<string>();

        public Logger(Config config)
        {
            _logFilePath = config.LogFilePath;
        }

        public void Log(string message)
        {
            logs.Add(message);
            Console.WriteLine($"LOG: {message}");
            SaveLogToFile(message);
        }

        private void SaveLogToFile(string message)
        {
            try
            {
                File.AppendAllText(_logFilePath, message + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving log: {ex.Message}");
            }
        }

        public void PrintLogs()
        {
            Console.WriteLine("--- Log History ---");
            foreach (var log in logs)
            {
                Console.WriteLine(log);
            }
        }

        public enum CustomerType
        {
            Standard,
            Premium
        }
    }

    class Order
    {
        public string CustomerName { get; set; }
        public int Quantity { get; set; }
        public CustomerType CustomerType { get; set; }

        public Order(string customerName, int quantity, CustomerType customerType)
        {
            CustomerName = customerName;
            Quantity = quantity;
            CustomerType = customerType;
        }
    }
}
