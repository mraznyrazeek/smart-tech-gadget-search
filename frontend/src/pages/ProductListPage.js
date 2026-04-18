import { useEffect, useMemo, useState } from "react";
import { getProducts, searchProducts } from "../services/productService";
import "./ProductListPage.css";

function ProductListPage() {
  const [products, setProducts] = useState([]);
  const [allProducts, setAllProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [searchTerm, setSearchTerm] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("");
  const [selectedBrand, setSelectedBrand] = useState("");

  const [selectedProduct, setSelectedProduct] = useState(null);

  useEffect(() => {
    const loadInitialProducts = async () => {
      try {
        setLoading(true);
        setError("");
        const data = await getProducts();
        setProducts(data);
        setAllProducts(data);
      } catch (err) {
        setError("Failed to load products.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    loadInitialProducts();
  }, []);

  useEffect(() => {
    const runSearch = async () => {
      try {
        setLoading(true);
        setError("");

        const hasFilters =
          searchTerm.trim() || selectedCategory.trim() || selectedBrand.trim();

        if (!hasFilters) {
          setProducts(allProducts);
        } else {
          const data = await searchProducts({
            query: searchTerm,
            category: selectedCategory,
            brand: selectedBrand,
          });
          setProducts(data);
        }
      } catch (err) {
        setError("Failed to search products.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    runSearch();
  }, [searchTerm, selectedCategory, selectedBrand, allProducts]);

  useEffect(() => {
    const handleEscape = (e) => {
      if (e.key === "Escape") {
        setSelectedProduct(null);
      }
    };

    window.addEventListener("keydown", handleEscape);
    return () => window.removeEventListener("keydown", handleEscape);
  }, []);

  const categories = useMemo(() => {
    return [...new Set(allProducts.map((p) => p.category).filter(Boolean))].sort();
  }, [allProducts]);

  const brands = useMemo(() => {
    return [...new Set(allProducts.map((p) => p.brand).filter(Boolean))].sort();
  }, [allProducts]);

  return (
    <div className="catalog-page">
      <div className="bg-orb orb-left"></div>
      <div className="bg-orb orb-right"></div>
      <div className="bg-grid"></div>

      <section className="hero">
        <div className="hero-left">
          <div className="hero-pill">Premium Search Experience</div>
          <h1>
            Discover Products with
            <span> Style, Speed & Precision</span>
          </h1>
          <p>
            A modern search platform built with React, .NET, PostgreSQL and Elasticsearch,
            designed for elegant browsing, fast discovery and a premium user experience.
          </p>
        </div>

        <div className="hero-right glass-panel">
          <div className="stat-card">
            <span className="stat-label">Products</span>
            <strong>{allProducts.length}</strong>
          </div>
          <div className="stat-card">
            <span className="stat-label">Categories</span>
            <strong>{categories.length}</strong>
          </div>
          <div className="stat-card">
            <span className="stat-label">Brands</span>
            <strong>{brands.length}</strong>
          </div>
        </div>
      </section>

      <section className="search-shell glass-panel">
        <div className="search-top">
          <div>
            <h2>Search Catalog</h2>
            <p>{loading ? "Searching..." : `${products.length} results available`}</p>
          </div>

          <button
            className="clear-btn"
            onClick={() => {
              setSearchTerm("");
              setSelectedCategory("");
              setSelectedBrand("");
              setError("");
            }}
          >
            Reset
          </button>
        </div>

        <div className="search-controls">
          <input
            type="text"
            placeholder="Search products, brands, categories..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />

          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
            className="search-select"
          >
            <option value="">All Categories</option>
            {categories.map((category) => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </select>

          <select
            value={selectedBrand}
            onChange={(e) => setSelectedBrand(e.target.value)}
            className="search-select"
          >
            <option value="">All Brands</option>
            {brands.map((brand) => (
              <option key={brand} value={brand}>
                {brand}
              </option>
            ))}
          </select>
        </div>

        {error && <p className="error-text">{error}</p>}
      </section>

      {loading && allProducts.length === 0 ? (
        <div className="loading-box glass-panel">Loading products...</div>
      ) : (
        <section className="product-grid">
          {products.map((product) => (
            <article
              key={product.id}
              className="product-card"
              onClick={() => setSelectedProduct(product)}
            >
              <div className="card-glow"></div>

              <div className="product-image-wrap">
                {product.thumbnailUrl ? (
                  <img
                    src={product.thumbnailUrl}
                    alt={product.name}
                    className="product-image"
                  />
                ) : (
                  <div className="product-image placeholder-image">No Image</div>
                )}
              </div>

              <div className="product-body">
                <div className="product-chip-row">
                  <span className="chip">{product.category}</span>
                  <span className="chip muted-chip">{product.brand}</span>
                </div>

                <h3>{product.name}</h3>

                <p className="description">
                  {product.description?.length > 95
                    ? `${product.description.substring(0, 95)}...`
                    : product.description}
                </p>

                <div className="product-meta">
                  <div className="price-area">
                    <span>Price</span>
                    <strong>${product.price}</strong>
                  </div>

                  <div className="side-meta">
                    <span>★ {product.rating ?? "N/A"}</span>
                    <span>Stock {product.stockQuantity}</span>
                  </div>
                </div>
              </div>
            </article>
          ))}
        </section>
      )}

      {selectedProduct && (
        <div className="modal-overlay" onClick={() => setSelectedProduct(null)}>
          <div className="modal-card" onClick={(e) => e.stopPropagation()}>
            <button className="modal-close" onClick={() => setSelectedProduct(null)}>
              ×
            </button>

            <div className="modal-content">
              <div className="modal-image-section">
                {selectedProduct.thumbnailUrl ? (
                  <img
                    src={selectedProduct.thumbnailUrl}
                    alt={selectedProduct.name}
                    className="modal-image"
                  />
                ) : (
                  <div className="modal-image placeholder-image">No Image</div>
                )}
              </div>

              <div className="modal-details">
                <div className="product-chip-row">
                  <span className="chip">{selectedProduct.category}</span>
                  <span className="chip muted-chip">{selectedProduct.brand}</span>
                </div>

                <h2>{selectedProduct.name}</h2>

                <p className="modal-description">{selectedProduct.description}</p>

                <div className="modal-stats">
                  <div className="modal-stat-box">
                    <span>Price</span>
                    <strong>${selectedProduct.price}</strong>
                  </div>

                  <div className="modal-stat-box">
                    <span>Rating</span>
                    <strong>{selectedProduct.rating ?? "N/A"}</strong>
                  </div>

                  <div className="modal-stat-box">
                    <span>Stock</span>
                    <strong>{selectedProduct.stockQuantity}</strong>
                  </div>

                  <div className="modal-stat-box">
                    <span>Brand</span>
                    <strong>{selectedProduct.brand}</strong>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProductListPage;