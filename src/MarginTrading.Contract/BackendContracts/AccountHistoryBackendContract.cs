﻿using System;

namespace MarginTrading.Contract.BackendContracts
{
    public class AccountHistoryBackendContract
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public string AccountId { get; set; }
        public string ClientId { get; set; }
        public decimal Amount { get; set; }
        public decimal Balance { get; set; }
        public decimal WithdrawTransferLimit { get; set; }
        public string Comment { get; set; }
        public AccountHistoryTypeContract Type { get; set; }
        public string OrderId { get; set; }
        public string LegalEntity { get; set; }
        public string AuditLog { get; set; }
        public decimal AmountInUsd { get; set; }
    }
}
