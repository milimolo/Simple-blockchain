using Simple_blockchain.models;
using System.Security.Cryptography;
using System.Text;

namespace Simple_blockchain
{
    public class BlockMiner
    {
        public List<Block>? Blockchain { get; set; }
        private TransactionPool? transactionPool;
        private CancellationTokenSource? cancellationToken;

        public static string CalculateHash(string rawData)
        {
            // Create a SHA256
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - Returns byte array
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert bute array to a string
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private string FindMerkleRootHash(IList<Transaction> transactionList)
        {
            var transactionStrList = transactionList.Select(tran => 
                CalculateHash(CalculateHash(tran.From + tran.To + tran.Amount))).ToList();
            return BuildMerkleRootHash(transactionStrList);
        }

        private string BuildMerkleRootHash(IList<string> merkleLeaves)
        {
            if (merkleLeaves == null || !merkleLeaves.Any())
                return string.Empty;

            if (merkleLeaves.Count() == 1)
                return merkleLeaves.First();

            if (merkleLeaves.Count() % 2 > 0)
                merkleLeaves.Add(merkleLeaves.Last());

            var merkleBranches = new List<string>();

            for (int i = 0; i < merkleLeaves.Count(); i += 2)
            {
                var leafPair = string.Concat(merkleLeaves[i], merkleLeaves[i + 1]);
                merkleBranches.Add(CalculateHash(CalculateHash(leafPair)));
            }
            return BuildMerkleRootHash(merkleBranches);
        }

        private void GenerateBlock()
        {
            var lastBlock = Blockchain.LastOrDefault();
            var block = new Block()
            {
                TimeStamp = DateTime.Now,
                Nounce = 0,
                TransactionList = transactionPool.TakeAll(),
                Index = (lastBlock?.Index + 1 ?? 0),
                PrevHash = lastBlock?.Hash ?? string.Empty
            };
            MineBlock(block);
            Blockchain.Add(block);
        }

        private void MineBlock(Block block)
        {
            var merkleRootHash = FindMerkleRootHash(block.TransactionList);
            long nounce = -1;
            var hash = string.Empty;
            do
            {
                nounce++;
                var rowData = block.Index + block.PrevHash + block.TimeStamp.ToString() + nounce + merkleRootHash;
                hash = CalculateHash(CalculateHash(rowData));
            }
            while (!hash.StartsWith("0000"));
            block.Hash = hash;
            block.Nounce = nounce;
        }

        private void DoGenerateBlock()
        {
            while (true)
            {
                var startTime = DateTime.Now.Millisecond;
                GenerateBlock();
                var endTime = DateTime.Now.Millisecond;
                var remainTime = 1000 - (endTime - startTime);
                Thread.Sleep(remainTime < 0 ? 0 : remainTime);
            }
        }

        public void Start()
        {
            cancellationToken = new CancellationTokenSource();
            Task.Run(() => DoGenerateBlock(), cancellationToken.Token);
            Console.WriteLine("Mining has started.");
        }

        public void Stop()
        {
            cancellationToken.Cancel();
            Console.WriteLine("Mining has stopped.");
        }
    }
}
