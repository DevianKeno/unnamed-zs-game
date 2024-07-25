using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

using UZSG.Items.Weapons;

namespace UZSG.FPP
{
    public class FPPViewmodelController : MonoBehaviour
    {
        [SerializeField] Transform armsHolder;
        [SerializeField] Transform weaponHolder;

        public struct LoadAssetReferenceContext
        {
            public GameObject GameObject { get; set; }
            public AsyncOperationStatus Status { get; set; }
        }

        public delegate void OnLoadAssetReferenceCompleted(LoadAssetReferenceContext context);
        public delegate void OnLoadViewmodelCompleted(Viewmodel viewmodel);

        public async Task<LoadAssetReferenceContext> LoadAssetReferenceAsync(AssetReference asset)
        {
            AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(asset);
            await handle.Task;
            return new LoadAssetReferenceContext
            {
                GameObject = handle.Status == AsyncOperationStatus.Succeeded ? handle.Result : null,
                Status = handle.Status
            };
        }

        public async void LoadViewmodelAsync(IFPPVisible obj, OnLoadViewmodelCompleted callback = null)
        {
            GameObject arms = null;
            GameObject weapon = null;

            var armsTask = Utils.IsAssetReferenceSet(obj.ArmsViewmodel) ? LoadAssetReferenceAsync(obj.ArmsViewmodel) : Task.FromResult(new LoadAssetReferenceContext());
            var weaponTask = Utils.IsAssetReferenceSet(obj.ModelViewmodel) ? LoadAssetReferenceAsync(obj.ModelViewmodel) : Task.FromResult(new LoadAssetReferenceContext());

            var armsResult = await armsTask;
            if (armsResult.Status == AsyncOperationStatus.Succeeded)
            {
                arms = Instantiate(armsResult.GameObject, armsHolder.transform);
            }

            var weaponResult = await weaponTask;
            if (weaponResult.Status == AsyncOperationStatus.Succeeded)
            {
                weapon = Instantiate(weaponResult.GameObject, weaponHolder.transform);
            }

            callback?.Invoke(new()
            {
                Arms = arms,
                Weapon = weapon,
                WeaponData = obj as WeaponData,
            });
        }

        public void ReplaceViewmodel(Viewmodel viewmodel)
        {
            if (armsHolder.childCount > 0)
            {
                var armsModel = armsHolder.GetChild(0);
                armsModel.gameObject.SetActive(false);

            }
            if (viewmodel.Arms != null)
            {
                viewmodel.Arms.SetActive(true);
            }
            
            if (weaponHolder.childCount > 0)
            {
                var weaponModel = weaponHolder.GetChild(0);
                weaponModel.gameObject.SetActive(false);
            }
            if (viewmodel.Weapon != null)
            {
                viewmodel.Weapon.SetActive(true);
            }
        }
    }
}