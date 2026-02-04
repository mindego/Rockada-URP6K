using System;
using crc32 = System.UInt32;

public class ObjId
{
    public string name;
    public crc32 obj_id;

    //ObjId() : name(0), obj_id(UndefinedID) { }
    //ObjId(crc32 id) : name(0), obj_id(id) { }
    //ObjId(cstr n) : name(n), obj_id(UndefinedID) { }
    //ObjId(cstr n, crc32 id) : name(n), obj_id(id) { }

    public override string ToString()
    {
        return String.Format("ObjId Text:[{0}] Code:[{1}]", name,obj_id.ToString("X8"));
    }
    public string cstr() { return name; }
    public crc32 crc32() { return obj_id; }

    public ObjId(string name, uint obj_id)
    {
        this.name = name;
        this.obj_id = obj_id;
    }

    public ObjId() : this(null, 0xFFFFFFFF) { }
    public ObjId(crc32 id) : this(null,id) {  }
    public ObjId(string n) : this(n, 0xFFFFFFFF) { }

    public static implicit operator string(ObjId id) => id.name;
    public static implicit operator crc32(ObjId id) => id.obj_id;
    
    public static implicit operator ObjId(string name) => new ObjId(name);
    public static implicit operator ObjId(crc32 id) => new ObjId(id);

}

