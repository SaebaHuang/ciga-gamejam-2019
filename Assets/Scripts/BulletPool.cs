using UnityEngine;
using System.Collections.Generic;

public class BulletPool : MonoBehaviour {
    public GameObject bulletPrefab;

    public Sprite enemyHeartBullet;

    public Sprite enemyCoinBullet;
    public Sprite characterHeartBullet;
    public Sprite characterCoinBullet;

    private List<GameObject> _pool;

    public void Get(Shape shape, bool isCharacter) {
        GameObject gottenObj = null;
        for (int i = 0; i < _pool.Count; ++i) {
            var obj = _pool[i];
            if (!obj.activeSelf) {
                gottenObj = obj;
                break;
            }
        }
        if (gottenObj == null) {
            // 没找到空闲，新建
            gottenObj = Instantiate(bulletPrefab, transform);
            _pool.Add(gottenObj);
        }

        Sprite sp = null;
        if (shape == Shape.COIN && isCharacter) {
            sp = characterCoinBullet;
        } else if (shape == Shape.HEART && isCharacter) {
            sp = characterHeartBullet;
        } else if (shape == Shape.COIN && !isCharacter) {
            sp = enemyCoinBullet;
        } else if (shape == Shape.HEART && !isCharacter) {
            sp = enemyHeartBullet;
        }
        gottenObj.GetComponent<SpriteRenderer>().sprite = sp;
    }
}