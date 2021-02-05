using System.Collections.Generic;
using System.Linq;

namespace UIWidgetsGallery.demo.shrine.model
{
    public static class AppStateModelUtils
    {
        public static readonly float _salesTaxRate = 0.06f;
        public static readonly float _shippingCostPerItem = 7.0f;
    }

    public class AppStateModel : Model
    {
        private List<Product> _availableProducts;

        private Category _selectedCategory = Category.all;

        private Dictionary<int, int> _productsInCart = new Dictionary<int, int> { };

        public Dictionary<int, int> productsInCart => new Dictionary<int, int>(this._productsInCart);

        public int totalCartQuantity => this._productsInCart.Values.Sum();

        public Category selectedCategory => this._selectedCategory;

        public float subtotalCost
        {
            get
            {
                var sum = 0;
                foreach (var id in this.productsInCart.Keys)
                    sum += this._availableProducts[id].price * this.productsInCart[id];
                return sum;
            }
        }

        public float shippingCost => AppStateModelUtils._shippingCostPerItem * this._productsInCart.Values.Sum();

        public float tax => this.subtotalCost * AppStateModelUtils._salesTaxRate;

        public float totalCost => this.subtotalCost + this.shippingCost + this.tax;


        public List<Product> getProducts()
        {
            if (this._availableProducts == null) return new List<Product>();

            if (this._selectedCategory == Category.all)
                return new List<Product>(this._availableProducts);
            else
                return this._availableProducts
                    .Where((Product p) => p.category == this._selectedCategory)
                    .ToList();
        }

        public void addProductToCart(int productId)
        {
            if (!this._productsInCart.ContainsKey(productId))
                this._productsInCart[productId] = 1;
            else
                this._productsInCart[productId]++;

            this.notifyListeners();
        }

        public void removeItemFromCart(int productId)
        {
            if (this._productsInCart.ContainsKey(productId))
            {
                if (this._productsInCart[productId] == 1)
                    this._productsInCart.Remove(productId);
                else
                    this._productsInCart[productId]--;
            }

            this.notifyListeners();
        }

        public Product getProductById(int id)
        {
            return this._availableProducts.First((Product p) => p.id == id);
        }

        public void clearCart()
        {
            this._productsInCart.Clear();
            this.notifyListeners();
        }

        public void loadProducts()
        {
            this._availableProducts = ProductsRepository.loadProducts(Category.all);
            this.notifyListeners();
        }

        public void setCategory(Category newCategory)
        {
            this._selectedCategory = newCategory;
            this.notifyListeners();
        }

        public override string ToString()
        {
            return $"AppStateModel(totalCost: {this.totalCost})";
        }
    }
}