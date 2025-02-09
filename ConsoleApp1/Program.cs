using System;
using System.Collections.Generic;
using System.IO;

namespace RefactoredOrderSystem
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new Config("orders.txt", "logs.txt");
            ILogger logger = new FileLogger(config.LogFilePath);
            IOrderRepository orderRepository = new FileOrderRepository(config.OrderFilePath);
            IPriceCalculator priceCalculator = new PriceCalculator();
            IDiscountService discountService = new DiscountService();
            var orderService = new OrderService(priceCalculator, discountService, orderRepository);
            var processor = new OrderProcessor(orderService, logger);

            processor.ProcessOrder("John Doe", 5, "standard");
            processor.ProcessOrder("Jane Doe", 15, "premium");
            processor.ProcessOrder("Alice", 2, "standard");
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

    interface ILogger
    {
        void Log(string message);
    }

    class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        public FileLogger(string logFilePath)
        {
            _logFilePath = logFilePath;
        }
        public void Log(string message)
        {
            File.AppendAllText(_logFilePath, message + "\n");
            Console.WriteLine($"LOG: {message}");
        }
    }

    interface IOrderRepository
    {
        void SaveOrder(string customer, int quantity, double price);
    }

    class FileOrderRepository : IOrderRepository
    {
        private readonly string _filePath;
        public FileOrderRepository(string filePath)
        {
            _filePath = filePath;
        }
        public void SaveOrder(string customer, int quantity, double price)
        {
            File.AppendAllText(_filePath, $"{customer}, {quantity}, {price}\n");
        }
    }

    interface IPriceCalculator
    {
        double CalculatePrice(int quantity);
    }

    class PriceCalculator : IPriceCalculator
    {
        public double CalculatePrice(int quantity)
        {
            double price = quantity * 10;
            return quantity > 10 ? price * 0.9 : price;
        }
    }

    interface IDiscountService
    {
        double ApplyDiscount(double price, string customerType);
    }

    class DiscountService : IDiscountService
    {
        public double ApplyDiscount(double price, string customerType)
        {
            return customerType == "premium" ? price * 0.85 : price;
        }
    }

    class OrderProcessor
    {
        private readonly OrderService _orderService;
        private readonly ILogger _logger;
        public OrderProcessor(OrderService orderService, ILogger logger)
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
        private readonly IPriceCalculator _calculator;
        private readonly IDiscountService _discountService;
        private readonly IOrderRepository _orderRepository;
        public OrderService(IPriceCalculator calculator, IDiscountService discountService, IOrderRepository orderRepository)
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
}
