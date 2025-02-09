using System;
using System.Collections.Generic;
using System.IO;

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

            processor.ProcessOrder("John Doe", 5, "standard");
            processor.ProcessOrder("Jane Doe", 15, "premium");
            processor.ProcessOrder("Alice", 2, "standard");

            logger.PrintLogs(); // Виведення усіх логів
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

        public void ProcessOrder(string customer, int quantity, string customerType)
        {
            double finalPrice = _orderService.HandleOrder(customer, quantity, customerType);
            Console.WriteLine($"Customer: {customer}, Quantity: {quantity}, Total Price: {finalPrice}");
            _logger.Log($"Processed order for {customer}, quantity: {quantity}, price: {finalPrice}, type: {customerType}");
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

        public double HandleOrder(string customer, int quantity, string customerType)
        {
            double price = _priceService.CalculatePrice(quantity);
            price = _discountService.ApplyDiscount(price, customerType);
            _orderPersistenceService.SaveOrder(customer, quantity, price);
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
        public double ApplyDiscount(double price, string customerType)
        {
            if (customerType == "premium")
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
    }
}
