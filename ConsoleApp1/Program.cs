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
            var orderService = new OrderService(new PriceCalculator(), new DiscountService(), new OrderRepository(config));
            var processor = new OrderProcessor(orderService, logger);

            processor.ProcessOrder("John Doe", 5, "standard");
            processor.ProcessOrder("Jane Doe", 15, "premium");
            processor.ProcessOrder("Alice", 2, "standard");

            logger.PrintLogs(); // Вивід усіх логів
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
        private readonly PriceCalculator _calculator;
        private readonly DiscountService _discountService;
        private readonly OrderRepository _orderRepository;

        public OrderService(PriceCalculator calculator, DiscountService discountService, OrderRepository orderRepository)
        {
            _calculator = calculator;
            _discountService = discountService;
            _orderRepository = orderRepository;
        }

        public double HandleOrder(string customer, int quantity, string customerType)
        {
            double price = _calculator.CalculatePrice(quantity);
            price = _discountService.ApplyDiscount(price, customerType);
            _orderRepository.SaveOrder(customer, quantity, price);
            return price;
        }
    }

    class OrderRepository
    {
        private readonly string _filePath;

        public OrderRepository(Config config)
        {
            _filePath = config.OrderFilePath;
        }

        public void SaveOrder(string customer, int quantity, double price)
        {
            File.AppendAllText(_filePath, $"{customer}, {quantity}, {price}\n");
        }
    }

    class PriceCalculator
    {
        public double CalculatePrice(int quantity)
        {
            double price = quantity * 10;
            if (quantity > 10)
            {
                price *= 0.9; 
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
                return price * 0.85; 
            }
            return price;
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
