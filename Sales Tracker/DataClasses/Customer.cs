namespace Sales_Tracker.DataClasses
{
    /// <summary>
    /// Represents a customer with contact information, rental history, payment status, and notes.
    /// </summary>
    public class Customer
    {
        // Getters and setters
        public string CustomerID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public bool IsBanned { get; set; } = false;
        public string BanReason { get; set; } = "";
        public DateTime? BanDate { get; set; } = null;
        public string Notes { get; set; } = "";
        public List<RentalRecord> RentalHistory { get; set; } = [];
        public PaymentStatus CurrentPaymentStatus { get; set; } = PaymentStatus.Current;
        public decimal OutstandingBalance { get; set; } = 0m;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastRentalDate { get; set; } = null;

        public enum PaymentStatus
        {
            Current,
            Overdue,
            Delinquent
        }

        // Default constructor required for deserialization
        public Customer() { }

        public Customer(string customerID, string name, string email, string phoneNumber, string address)
        {
            CustomerID = customerID;
            Name = name;
            Email = email;
            PhoneNumber = phoneNumber;
            Address = address;
        }

        /// <summary>
        /// Adds a rental record to the customer's history.
        /// </summary>
        public void AddRentalRecord(RentalRecord record)
        {
            RentalHistory.Add(record);
            LastRentalDate = record.RentalDate;
        }

        /// <summary>
        /// Bans the customer with a specified reason.
        /// </summary>
        public void Ban(string reason)
        {
            IsBanned = true;
            BanReason = reason;
            BanDate = DateTime.Now;
        }

        /// <summary>
        /// Unbans the customer.
        /// </summary>
        public void Unban()
        {
            IsBanned = false;
            BanReason = "";
            BanDate = null;
        }

        /// <summary>
        /// Updates the customer's payment status based on outstanding balance and overdue dates.
        /// </summary>
        public void UpdatePaymentStatus()
        {
            if (OutstandingBalance <= 0)
            {
                CurrentPaymentStatus = PaymentStatus.Current;
            }
            else if (OutstandingBalance > 0)
            {
                // Check if any rental is overdue
                bool hasOverdueRentals = RentalHistory.Any(r => 
                    r.DueDate.HasValue && 
                    r.DueDate.Value < DateTime.Now && 
                    !r.IsReturned);

                CurrentPaymentStatus = hasOverdueRentals ? PaymentStatus.Overdue : PaymentStatus.Current;
            }
        }
    }

    /// <summary>
    /// Represents a single rental transaction for a customer.
    /// </summary>
    public class RentalRecord
    {
        public string RentalID { get; set; }
        public string ProductName { get; set; }
        public DateTime RentalDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public bool IsReturned { get; set; } = false;
        public decimal RentalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public string Notes { get; set; } = "";

        public RentalRecord() { }

        public RentalRecord(string rentalID, string productName, DateTime rentalDate, DateTime? dueDate, decimal rentalAmount)
        {
            RentalID = rentalID;
            ProductName = productName;
            RentalDate = rentalDate;
            DueDate = dueDate;
            RentalAmount = rentalAmount;
        }
    }
}