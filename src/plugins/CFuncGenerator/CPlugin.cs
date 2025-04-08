namespace CFuncGenerator;

public class CPlugin : IndependentPlugin
{
    public override IReturn Start(IParams param)
    {
        IReturn res = new IReturn();

        CGenerator generator = new CGenerator(param);
        generator.GenerateAll();

        return res;
    }

    public override bool NeedParams
    {
        get { return true; }
    }
}