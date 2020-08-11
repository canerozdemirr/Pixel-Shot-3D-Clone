using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CubeController : MonoBehaviour
{
    private MeshRenderer mr;
    private Color currentColor;

    public GameObject explosionParticle;

    private void Awake()
    {
        mr = GetComponent<MeshRenderer>();
        currentColor = mr.material.color;
        var main = explosionParticle.GetComponent<ParticleSystem>().main;
        main.startColor = currentColor;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag(Tags.GroundTag))
        {
            GameManager.Instance.allObstacles.Remove(gameObject);
            GameManager.Instance.cubeNumberSlider.value++;
            Instantiate(explosionParticle, transform.position, Quaternion.identity);
            Invoke(nameof(DeActivateParticleEffect), 1f);
            Destroy(gameObject);
        }

        else if (other.gameObject.CompareTag(Tags.CubeTag) || other.gameObject.CompareTag(Tags.BallTag))
        {
            if (!gameObject.GetComponent<Rigidbody>())
            {
                var rb = gameObject.AddComponent<Rigidbody>();
                const RigidbodyConstraints constraints = (RigidbodyConstraints) 72;
                rb.constraints = constraints;
                var magnitude = GameManager.Instance.cubePushPower;
                var force = transform.position - other.transform.position;
                force.Normalize();
                other.gameObject.GetComponent<Rigidbody>().AddForce(force * magnitude);
                mr.material.DOColor(Color.white, 1f)
                    .OnComplete(ChangeTheColorBack);
            }
        }
    }

    private void ChangeTheColorBack()
    {
        if (mr) mr.material.DOColor(currentColor, 1f);
    }

    private void DeActivateParticleEffect()
    {
        explosionParticle.SetActive(false);
    }
}