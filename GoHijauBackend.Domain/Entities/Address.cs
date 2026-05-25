namespace GoHijauBackend.Domain.Entities
{
    public class Address
    {
        public string UnitNo { get; private set; }
        public string Street { get; private set; }
        public string District { get; private set; }
        public string Postcode { get; private set; }
        public string State { get; private set; }
        public string Country { get; private set; }

        public Address(string unitNo, string street, string district, string postcode, string state, string country)
        {
            UnitNo = unitNo;
            Street = street;
            District = district;
            Postcode = postcode;
            State = state;
            Country = country;
        }
    }
}
