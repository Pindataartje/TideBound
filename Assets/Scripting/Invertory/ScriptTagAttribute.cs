using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ScriptTagAttribute : Attribute
{
    public string Tag { get; private set; }

    public ScriptTagAttribute(string tag)
    {
        Tag = tag;
    }
}
