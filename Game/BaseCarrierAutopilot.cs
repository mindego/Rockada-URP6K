public class BaseCarrierAutopilot
{
    public BaseCarrierAutopilot(BaseScene s, BaseCarrier c)
    {
        myScene = s;
        myCarrier = c;
    }
    public virtual void update(float PacketDelay, CarrierUpdatePacket pkt) { }
    public virtual void move(float scale) { }
    protected BaseScene myScene;
    protected BaseCarrier myCarrier;
};

public class LocalCarrierAutopilot : BaseCarrierAutopilot
{
	public LocalCarrierAutopilot(BaseScene s, BaseCarrier c) : base(s, c) { }
	public override void move(float scale)
	{
		myCarrier.updatePhysic(scale);
	}
};


//struct RemoteCarrierAutopilot : BaseCarrierAutopilot
//{
//	RemoteCarrierAutopilot(BaseScene& s, BaseCarrier* c) : BaseCarrierAutopilot(s, c),mSmoother(*(c->pFPO)) {}
//	virtual void update(float PacketDelay,const CarrierUpdatePacket* pPkt) {
//		if (PacketDelay<myScene.GetLocalDelay()*.75) {
//			mSmoother.SetScale(myScene.GetTime(),myScene.GetTime()+myScene.GetLocalDelay()-PacketDelay);
//			MATRIX TgtPos;
//	TgtPos.Angles2Vectors(pPkt->HeadingAngle,pPkt->PitchAngle,0);
//			mSmoother.SetOrg(myCarrier->pFPO->Org,myCarrier->mySpeed,pPkt->Org,pPkt->Speed);
//			mSmoother.SetOrientation(*(myCarrier->pFPO),TgtPos);
//		}
//	}
//	virtual void move(float)
//{
//	float s = mSmoother.GetScale(myScene.GetTime());
//	mSmoother.GetOrg(myCarrier->pFPO->Org, s);
//	mSmoother.GetSpeed(myCarrier->mySpeed, s);
//	mSmoother.GetOrientation(*(myCarrier->pFPO), s);
//}
//private:
//	SmootherOrientation<SmootherOrg1> mSmoother;
//};