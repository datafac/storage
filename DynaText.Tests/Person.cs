namespace DTOMaker.Gentime.Tests
{
    internal class Person : DynaTextBase
    {
        public int Id { get { return _map.Get(nameof(Id), 0); } set { _map.Set(nameof(Id), value); } }
        public string Surname { get { return _map.Get(nameof(Surname), ""); } set { _map.Set(nameof(Surname), value); } }
        public string? Nickname { get { return _map.Get<string?>(nameof(Nickname), null); } set { _map.Set(nameof(Nickname), value); } }
        public ushort? YOB { get { return _map.Get<ushort?>(nameof(YOB), null); } set { _map.Set(nameof(YOB), value); } }
        public string?[] OtherNames
        {
            get { return _map.GetArray<string>(nameof(OtherNames), ""); }
            set { _map.SetArray<string>(nameof(OtherNames), value); }
        }
    }
}