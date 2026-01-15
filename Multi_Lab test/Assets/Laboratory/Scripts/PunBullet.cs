using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PhotonTransformView))]
[RequireComponent(typeof(PhotonRigidbodyView))]
public class PunBullet : PunBaseInstance { 
    
    public float BulletForce = 20f;
    [Range(1, 10)]
    public int m_AmountDamage = 5;
    private float damage;
    private int ownerViewID;

    protected override void PunInstantiateObject(PhotonMessageInfo info)
    {
        base.PunInstantiateObject(info);
        
        // ===== รับข้อมูลจาก Photon Instantiate =====
        if (info.photonView.InstantiationData != null)
        {
            object[] data = info.photonView.InstantiationData;
            ownerViewID = (int)data[0];
            damage = (float)data[1];
        }
        else
        {
            Debug.LogWarning("No InstantiationData found");
            damage = 5f; // fallback
        }

        //info.sender.TagObject = this.GameObject;
        Rigidbody bullet = GetComponent<Rigidbody>();
        // Add velocity to the bullet
        bullet.linearVelocity = bullet.transform.forward * BulletForce;

        if (!photonView.IsMine)
            return;

        // Destroy the bullet after 10 seconds
        Destroy(this.gameObject, 10.0f);
    }

    protected override void TriggerWithEnvironment(Collider other)
    {
        PunHealth tempHealthOther = other.gameObject.GetComponent<PunHealth>();
        if (tempHealthOther != null)
            tempHealthOther.TakeDamage(m_AmountDamage, OwnerViewID);
        else Debug.Log("Empty Component.");
        
        PunRPCsNetworkAction scaleAction =
            other.GetComponent<PunRPCsNetworkAction>();

        if (scaleAction != null)
        {
            scaleAction.RequestScaleUp();
        }
        
    }
    protected override void TriggerWithPlayer(Collider other)
    {
        base.TriggerWithPlayer(other);

        PunHealth tempHealthOther = other.gameObject.GetComponent<PunHealth>();
        if (tempHealthOther != null)
            tempHealthOther.TakeDamage(m_AmountDamage, OwnerViewID);
        else Debug.Log("Empty Component.");
    }
    
}
