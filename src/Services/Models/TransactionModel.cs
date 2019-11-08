using StockportGovUK.NetStandard.Models.Models.RevsAndBens;

namespace revs_bens_service.Services.Models
{
    public class TransactionModelExtension : TransactionModel
    {
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            var transaction = (TransactionModel)obj;

            return Amount.Equals(transaction.Amount)
                && string.Equals(Date, transaction.Date)
                && string.Equals(Type, transaction.Type);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Amount.GetHashCode();
                hashCode = (hashCode * 397) ^ (Date != null ? Date.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
