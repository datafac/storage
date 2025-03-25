namespace DTOMaker.Gentime.Tests
{
    internal class Family : DynaTextBase
    {
        public Person? Leader {
            get { return _map.GetObject<Person>(nameof(Leader)); }
            set { _map.SetObject<Person>(nameof(Leader), value); }
        }

        public Person?[] Members
        {
            get { return _map.GetVector<Person>(nameof(Members), null); }
            set { _map.SetVector<Person>(nameof(Members), value); }
        }
    }
}