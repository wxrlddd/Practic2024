using System;
using System.Collections.Generic;
using System.IO;

namespace CodeSmellsExample
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderProcessor processor = new OrderProcessor(new PriceCalculator(), new Logger(), new DiscountService());
            processor.ProcessOrder("John Doe", 5, "standard");
            processor.ProcessOrder("Jane Doe", 15, "premium");
            processor.ProcessOrder("Alice", 2, "standard");
        }
    }

    class OrderProcessor
    {
        private readonly PriceCalculator _calculator;
        private readonly Logger _logger;
        private readonly DiscountService _discountService;
        private List<string> processedOrders = new List<string>();

        public OrderProcessor(PriceCalculator calculator, Logger logger, DiscountService discountService)
        {
            _calculator = calculator;
            _logger = logger;
            _discountService = discountService;
        }

        public void ProcessOrder(string customer, int quantity, string customerType)
        {
            double price = _calculator.CalculatePrice(quantity);
            price = _discountService.ApplyDiscount(price, customerType);
            Console.WriteLine($"Customer: {customer}, Quantity: {quantity}, Total Price: {price}");
            _logger.Log($"Processed order for {customer}, quantity: {quantity}, price: {price}, type: {customerType}");
            SaveToFile(customer, quantity, price);
        }

        private void SaveToFile(string customer, int quantity, double price)
        {
            File.WriteAllText("orders.txt", $"{customer}, {quantity}, {price}\n");
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
        private List<string> logs = new List<string>();

        public void Log(string message)
        {
            logs.Add(message);
            Console.WriteLine($"LOG: {message}");
        }
    }
}
