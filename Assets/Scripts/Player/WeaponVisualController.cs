using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeaponVisualController : MonoBehaviour
{
    private Animator anim;
    private bool isEquippingWeapon;
    private Player player;
    [SerializeField] private WeaponModel[] weaponModels;
    [SerializeField] private BackupWeaponModel[] backupWeaponModels;
    [Header("Rig")]
    [SerializeField] private float rigWeightIncreaseRate;
    private Rig rig;
    private bool shouldIncrease_RigWeight;

    [Header("Left hand IK")]
    [SerializeField] private TwoBoneIKConstraint leftHandIK;
    [SerializeField] private Transform leftHandIK_Target;
    [SerializeField] private float leftHandIKWeightIncreaseRate;
    private bool shouldIncrease_LeftHandIKWeight;

    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
        rig = GetComponentInChildren<Rig>();
        weaponModels = GetComponentsInChildren<WeaponModel>(true);
        player = GetComponent<Player>();
        backupWeaponModels = GetComponentsInChildren<BackupWeaponModel>(true);
    }
    private void Update()
    {
        UpdateRigWeight();
        UpdateLeftHandIKWeight();
    }
    public WeaponModel CurrentWeaponModel()
    {
        WeaponModel weaponModel = null;
        WeaponType weaponType = player.weaponController.GetCurrentWeapon().weaponType;
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i].weaponType == weaponType)
            {
                weaponModel = weaponModels[i];
                break;
            }
        }
        return weaponModel;

    }

    public void PlayReloadAnimation()
    {
        if (isEquippingWeapon) return;
        float reloadSpeed = player.weaponController.GetCurrentWeapon().reloadSpeed;
        anim.SetTrigger("Reload");
        anim.SetFloat("ReloadSpeed", reloadSpeed);
        ReduceRigWeight();
    }

    private void UpdateLeftHandIKWeight()
    {
        if (shouldIncrease_LeftHandIKWeight)
        {
            leftHandIK.weight += leftHandIKWeightIncreaseRate * Time.deltaTime;
            if (leftHandIK.weight >= 1)
            {
                shouldIncrease_LeftHandIKWeight = false;
            }
        }
    }

    private void UpdateRigWeight()
    {
        if (shouldIncrease_RigWeight)
        {
            rig.weight += rigWeightIncreaseRate * Time.deltaTime;
            if (rig.weight >= 1)
            {
                shouldIncrease_RigWeight = false;
            }
        }
    }

    private void ReduceRigWeight()
    {
        rig.weight = .15f;
    }

    public void PlayWeaponEquipAnimation()
    {
        EquipType equipType = CurrentWeaponModel().equipAnimationType;
        float equipmentSpeed = player.weaponController.GetCurrentWeapon().equipementSpeed;
        leftHandIK.weight = 0;
        ReduceRigWeight();
        anim.SetFloat("EquipType", (float)equipType);
        anim.SetTrigger("EquipWeapon");
        anim.SetFloat("EquipSpeed", equipmentSpeed);
        SetBusyEquippingWeaponTo(true);
    }

    public void SetBusyEquippingWeaponTo(bool busy)
    {
        isEquippingWeapon = busy;
        anim.SetBool("BusyEquippingWeapon", isEquippingWeapon);
    }

    public void MaximizeRigWeight()
    {
        shouldIncrease_RigWeight = true;
    }
    public void MaximizeLeftHandWeight()
    {
        shouldIncrease_LeftHandIKWeight = true;
    }
    private void SwitchOffBackupWeaponModels()
    {
        foreach (BackupWeaponModel backupWeaponModel in backupWeaponModels)
        {
            backupWeaponModel.gameObject.SetActive(false);
        }
    }
    public void SwitchOnBackupWeaponModel()
    {
        WeaponType weaponType = player.weaponController.BackupWeapon().weaponType;
        foreach (BackupWeaponModel backupWeaponModel in backupWeaponModels)
        {
            if (backupWeaponModel.weaponType == weaponType)
            {
                backupWeaponModel.gameObject.SetActive(true);
                break;
            }
        }
    }

    public void SwitchOnCurrentWeaponModel()
    {
        int animationIndex = (int)CurrentWeaponModel().holdType;

        SwitchOffWeaponModels();

        SwitchOffBackupWeaponModels();
        if (player.weaponController.HasOnlyOneWeapon() == false)
        {
            SwitchOnBackupWeaponModel();
        }

        SwitchAnimatorLayer(animationIndex);
        CurrentWeaponModel().gameObject.SetActive(true);
        AttackLeftHand();
    }

    public void SwitchOffWeaponModels()
    {
        for (int i = 0; i < weaponModels.Length; i++)
        {
            weaponModels[i].gameObject.SetActive(false);
        }
    }
    private void AttackLeftHand()
    {
        Transform targetTransform = CurrentWeaponModel().holdPoint;
        leftHandIK_Target.localPosition = targetTransform.localPosition;
        leftHandIK_Target.localRotation = targetTransform.localRotation;
    }
    private void SwitchAnimatorLayer(int layerIndex)
    {
        for (int i = 0; i < anim.layerCount; i++)
        {
            anim.SetLayerWeight(i, 0);
        }
        anim.SetLayerWeight(layerIndex, 1);
    }

}


