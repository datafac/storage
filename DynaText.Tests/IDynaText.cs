namespace DTOMaker.Gentime.Tests
{
    public interface IDynaText : IEmitText, ILoadText
    {
        DynaMap GetMap();
        void LoadFrom(DynaMap map);
    }
}