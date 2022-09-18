﻿namespace Simple_blockchain.models
{
    public class Block
    {
        public long Index { get; set; }
        public DateTime TimeStamp { get; set; }
        public string? Hash { get; set; }
        public string? PrevHash { get; set; }
        public long Nounce { get; set; }
        public List<Transaction>? TransactionList { get; set; }
    }
}
