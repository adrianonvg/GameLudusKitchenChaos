using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliveryManager : MonoBehaviour {


    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;


    public static DeliveryManager Instance { get; private set; }


    [SerializeField] private RecipeListSO recipeListSO;


    private List<RecipeSO> waitingRecipeSOList;
    private float spawnRecipeTimer;
    private float spawnRecipeTimerMax = 4f;
    private int waitingRecipesMax = 4;
    public int successfulRecipesAmount;
    [SerializeField] public float extraTimeOnSuccess = 2f;
    [SerializeField] public float timePenaltyOnFailure = 3f;
    [SerializeField] public int maxTotalRecipes = 10;
    public int totalRecipesGenerated = 0;


    public int recipesToWin = 5;


    [SerializeField] private KitchenGameManager kitchenGameManager;





    private void Awake() {
        Instance = this;


        waitingRecipeSOList = new List<RecipeSO>();
    }

    public void Start(){
        kitchenGameManager = KitchenGameManager.Instance;
    }

    private void Update() {
        if (KitchenGameManager.Instance.IsGamePlaying() && waitingRecipeSOList.Count < waitingRecipesMax && totalRecipesGenerated < maxTotalRecipes)
        {
            spawnRecipeTimer -= Time.deltaTime;
            if (spawnRecipeTimer <= 0f)
            {
                RecipeSO waitingRecipeSO = recipeListSO.recipeSOList[UnityEngine.Random.Range(0, recipeListSO.recipeSOList.Count)];
                waitingRecipeSOList.Add(waitingRecipeSO);
                totalRecipesGenerated++;
                OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
                spawnRecipeTimer = spawnRecipeTimerMax;
            }
        }
    }

    public void DeliverRecipe(PlateKitchenObject plateKitchenObject) {
        for (int i = 0; i < waitingRecipeSOList.Count; i++) {
            RecipeSO waitingRecipeSO = waitingRecipeSOList[i];

            if (waitingRecipeSO.kitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count) {
                // Has the same number of ingredients
                bool plateContentsMatchesRecipe = true;
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.kitchenObjectSOList) {
                    // Cycling through all ingredients in the Recipe
                    bool ingredientFound = false;
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList()) {
                        // Cycling through all ingredients in the Plate
                        if (plateKitchenObjectSO == recipeKitchenObjectSO) {
                            // Ingredient matches!
                            ingredientFound = true;
                            break;
                        }
                    }
                    if (!ingredientFound) {
                        // This Recipe ingredient was not found on the Plate
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe) {
                    // Player delivered the correct recipe!
                    
                    successfulRecipesAmount++;
                    float timeToAdd = Mathf.Min(extraTimeOnSuccess, kitchenGameManager.gamePlayingTimerMax - kitchenGameManager.gamePlayingTimer);
                    kitchenGameManager.gamePlayingTimer += timeToAdd;

                    if (kitchenGameManager.gamePlayingTimer > kitchenGameManager.gamePlayingTimerMax) {
                        kitchenGameManager.gamePlayingTimer = kitchenGameManager.gamePlayingTimerMax;
                    }

                    waitingRecipeSOList.RemoveAt(i);

                    OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
                    OnRecipeSuccess?.Invoke(this, EventArgs.Empty);

                    if (waitingRecipeSOList.Count == 0) {
                        ClearWaitingRecipes();
                    }
                    
                    
                    if (successfulRecipesAmount >= recipesToWin) {
                        KitchenGameManager.Instance.IsGameWin();
                        //Debug.Log("GANHOU A FASE!!!");
                    }

                    return;
                }
            }
        }

        // No matches found!
        // Player did not deliver a correct recipe
        kitchenGameManager.gamePlayingTimer -= timePenaltyOnFailure;
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    public List<RecipeSO> GetWaitingRecipeSOList() {
        return waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount() {
        return successfulRecipesAmount;
    }

    private void ClearWaitingRecipes() {
        waitingRecipeSOList.Clear();
        totalRecipesGenerated = 0;
    }

    

}
