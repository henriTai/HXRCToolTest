using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseInputNode : BaseNode {

    public virtual string GetResult()
    {
        return "None";
    }

    public override void DrawCurves()
    {
    }


}
