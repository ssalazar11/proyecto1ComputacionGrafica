using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public int HP=100;
    public void TakeDamage(int damageAmount){
        HP-=damageAmount;

        if (HP <=0){
            print("Player Dead");
            PlayerDead();
        }
        else{
            print("Player Hit");
        }
    }

    private void PlayerDead(){
        GetComponent<PlayerLook>().enabled=false;
        GetComponent<PlayerMovement>().enabled=false;

        GetComponentInChildren<Animator>().enabled=true;
    }



    private void OnTriggerEnter(Collider other){
        if(other.CompareTag("ZombieHand")){
            TakeDamage(25);
        }
    }
}
