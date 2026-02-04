using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//public class FpoController : MonoBehaviour
//{
//    private FpoData fpoData;
//    [Range(0,3)]
//    public int variant;
//    private int currentVariant = -1;
//    //[Range(0, 3)]
//    //public int LOD;
//    //private int currentLOD=-1;
//    public Mesh currentMesh;
//    public Mesh currentCollider;
//    private Material[] currentMaterials;
//    public Vector3 e1;
//    public Vector3 e2;
//    public Vector3 e3;
//    //public Vector3 org;

//    public Quaternion addRotation = Quaternion.Euler(0, 0, 0);



//    public Vector3 parentE1, parentE2, parentE3;

//    public Vector3 Dir, Up,Left;
//    public Vector3 transformDir, transformUp;

//// Update is called once per frame
//    void FixedUpdate()
//    {
//        if (variant != currentVariant) SetGraph(variant);
//        //transform.localRotation = Quaternion.LookRotation(e3, e2) * addRotation;
//        //transform.rotation = Quaternion.LookRotation(transform.root.rotation * e3, transform.root.rotation* e2) * addRotation;
  
//     }

//    public void SetFpoData(FpoData fpoData)
//    {
//        this.fpoData = fpoData;
//        e1 = fpoData.pos.e1;
//        e1.y *= -1;
//       // e1 *= -1;
//        e2 = fpoData.pos.e2;
//       // e2.y *= -1;
//        e2 *= -1;

//        e3 = Vector3.Cross(e1, e2);
//        Dir = e3;

//        Up = e2;
//        Left = e1;

//        transformDir = transform.forward;
//        transformUp = transform.up;

//        if (transform.parent == null) return;
//        Dir = transform.parent.rotation * Dir;
//        Up = transform.parent.rotation * Up;
//        Left = transform.parent.rotation * Left;
//        FpoController fpoParent = transform.parent.gameObject.GetComponent<FpoController>();
//        if (fpoParent == null) return;

//        parentE1 = fpoParent.e1;
//        parentE2 = fpoParent.e2;
//        parentE3 = fpoParent.e3;
//    }

//    public void SetGraph(int newVariant)
//    {
//        if (fpoData == null) return;
//        if (currentVariant == newVariant) return;

//        Debug.Log("Switching from " + currentVariant + " to " + newVariant);
//        variant = newVariant;
//        currentVariant = variant;

//        Debug.Log("Lod id: " + fpoData.images[variant].graph.ToString("X8"));
//        FpoGraphData fpoGraphData = GameDataHolder.GetResource<FpoGraphData>(PackType.MeshDB, fpoData.images[variant].graph);
//        if (fpoGraphData == null) return;
//        StormMesh graph = GameDataHolder.GetResource<StormMesh>(PackType.MeshDB, fpoGraphData.GetLod(0));
        
//        Mesh tmpMesh= StormMeshImport.ExtractMesh(graph);
//        currentMesh = tmpMesh;
//        currentCollider = tmpMesh;
//        currentMaterials = StormMeshImport.GetMaterials(graph);
//        UpdateImage();
//    }

//    public void SetGraphOld(int newVariant)
//    {
//        Debug.Log("Switching from " + currentVariant + " to " + newVariant);
//        //if (currentVariant == variant) return;
//        variant = newVariant;
//        currentVariant = variant;
//        GameObject tmpGameObject;

//        FpoGraphData fpoGraphData = GameDataHolder.GetResource<FpoGraphData>(PackType.MeshDB, fpoData.images[variant].graph);
//        if (fpoGraphData==null)
//        {
//            tmpGameObject = new GameObject();
//            tmpGameObject.AddComponent<MeshFilter>();
//            tmpGameObject.AddComponent<MeshRenderer>();
//        } else
//        {
//            tmpGameObject = GameDataHolder.GetResource<GameObject>(PackType.MeshDB, fpoGraphData.GetLod(0));
//        }
//        //GameObject tmpGameObject = GameDataHolder.GetResource<GameObject>(PackType.MeshDB, fpoData.images[variant].graph);
//        //tmpGameObject = GameDataHolder.GetResource<GameObject>(PackType.MeshDB, fpoGraphData.GetLod(0));

//        MeshFilter mf = tmpGameObject.GetComponent<MeshFilter>();
//        MeshRenderer mr = tmpGameObject.GetComponent<MeshRenderer>();
//        currentMesh = mf.mesh;
//        currentMaterials = mr.materials;
//        UpdateImage();
//        Destroy(tmpGameObject);
//    }

//    public void UpdateImage()
//    {
//        MeshFilter mf = GetComponent<MeshFilter>();
//        MeshRenderer mr = GetComponent<MeshRenderer>();
//        MeshCollider mc = GetComponent<MeshCollider>();

//        if (mf == null) mf = this.gameObject.AddComponent<MeshFilter>();
//        if (mr == null) mr = this.gameObject.AddComponent<MeshRenderer>();
//        if (mc == null) mc = this.gameObject.AddComponent<MeshCollider>();
//        mc.cookingOptions = MeshColliderCookingOptions.None;
//        //mc.convex = true;
//        mc.convex = false;

//        //mf.mesh = this.currentMesh;
//        mf.sharedMesh = this.currentMesh;
//        mr.materials = this.currentMaterials;
//        mc.sharedMesh = this.currentCollider;
        
//    }


//    void OnCollisionEnter(Collision collision)
//    {
//        foreach (ContactPoint contact in collision.contacts)
//        {
//            Debug.DrawRay(contact.point, contact.normal, Color.white);
//            Debug.Log("Hit on" + contact.point + "with + " + contact.otherCollider.transform.root.name );
//            Debug.DrawLine(contact.point, contact.otherCollider.transform.position, Color.yellow);
//            //GameObject explosion = new GameObject("Explosion");
//            //explosion.transform.position = contact.point;
//        }
//        if (collision.relativeVelocity.magnitude > 2)
//        {
//            Debug.Log("Heavy hit!");
            
//         }
                
//    }

//}
