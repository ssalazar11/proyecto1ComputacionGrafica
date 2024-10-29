using System.Collections;
using System.Collections.Generic;
using UnityEngine; 

public class Weapon : MonoBehaviour
{

    public int weaponDamage;

    //shooting
    public bool isShooting, readyToShoot;
    bool allowReset=true;
    public float shootingDelay=2f;

    //burst
    public int bulletsPerBurst=3;
    public int burstBulletsLeft;

    //spread
    public float spreadIntensity;



    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity=30;
    public float bulletPrefabLifeTime=3f;

    public GameObject muzzleEffect;
    private Animator animator;

    //loading
    public float reloadTime;
    public int magazineSize, bulletsLeft;
    public bool isReloading;

    public Vector3 spawnPosition;
    public Vector3 spawnRotation;
 
    public enum WeaponModel{
        Pistol1911,
        M16
    }

    public WeaponModel thisWeaponModel;

    public enum ShootingMode{
        Single,
        Burst,
        Auto
    }
    
    public ShootingMode currentShootingMode;

    private void Awake(){
        readyToShoot=true;
        burstBulletsLeft=bulletsPerBurst;
        animator=GetComponent<Animator>();

        bulletsLeft=magazineSize;
    }


    // Update is called once per frame
    void Update()
    {
        if (bulletsLeft==0&&isShooting){
            SoundManager.Instance.emptyMagazineSound1911.Play();
        }


        if(currentShootingMode==ShootingMode.Auto){
            isShooting=Input.GetKey(KeyCode.Mouse0);
        }
        else if(currentShootingMode==ShootingMode.Single||currentShootingMode==ShootingMode.Burst){
            isShooting=Input.GetKeyDown(KeyCode.Mouse0);
        }

        if (Input.GetKeyDown(KeyCode.R)&&bulletsLeft < magazineSize && isReloading==false){
            Reload();
        }

        //Automatic reload when magazine is empty
        if (readyToShoot && isShooting==false && isReloading==false && bulletsLeft<=0){
            Reload();
        }


        if(readyToShoot&&isShooting&&bulletsLeft>0 ){
            burstBulletsLeft=bulletsPerBurst;
            FireWeapon();
        }

        if (AmmoManager.Instance.ammoDisplay!=null){
            AmmoManager.Instance.ammoDisplay.text=$"{bulletsLeft/bulletsPerBurst}/{magazineSize/bulletsPerBurst}";
        }
    }

    private void FireWeapon(){

        bulletsLeft--;

        muzzleEffect.GetComponent<ParticleSystem>().Play();
        animator.SetTrigger("RECOIL");
 
        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot=false;
        Vector3 shootingDirection=CalculateDirectionAndSpread().normalized;
        //instantiate bullet
        GameObject bullet=Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        Bullet bul=bullet.GetComponent<Bullet>();
        bul.bulletDamage=weaponDamage;

        bullet.transform.forward=shootingDirection;
        //Shoot the bullet
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);


        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        if(allowReset){
            Invoke("ResetShot", shootingDelay);
            allowReset=false;
        }

        //burst mode
        if (currentShootingMode==ShootingMode.Burst&&burstBulletsLeft>1){
            burstBulletsLeft--;
            Invoke("FireWeapon", shootingDelay);
        }
    }

    private void Reload(){
        SoundManager.Instance.PlayReloadSound(thisWeaponModel);

        animator.SetTrigger("RELOAD");

        isReloading=true;
        Invoke("ReloadCompleted", reloadTime);
    }

    private void ReloadCompleted(){
        bulletsLeft=magazineSize;
        isReloading=false;
    }

    private void ResetShot(){
        readyToShoot=true;
        allowReset=true;
    }

    public Vector3 CalculateDirectionAndSpread(){
        //Shooting from the middle of the screen to check where we are pointing
        Ray ray=Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if(Physics.Raycast(ray, out hit)){
            //hitting something
            targetPoint=hit.point;
        }
        else
        {
            //shooting at the air
            targetPoint=ray.GetPoint(100);
        }
        Vector3 direction=targetPoint-bulletSpawn.position;

        float x=UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y=UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        //Returning the shooting direction and spread
        return direction+new Vector3(x, y, 0);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay){
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }
}
