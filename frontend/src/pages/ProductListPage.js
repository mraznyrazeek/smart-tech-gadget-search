import { useEffect, useMemo, useState } from "react";
import { getProducts, searchProducts } from "../services/productService";
import "./ProductListPage.css";

function ProductListPage() {
  const [products, setProducts] = useState([]);
  const [allProducts, setAllProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [searchTerm, setSearchTerm] = useState("");
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState("");
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
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
    }, 350);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    const runSearch = async () => {
      try {
        setLoading(true);
        setError("");

        const trimmedSearch = debouncedSearchTerm.trim();
        const trimmedCategory = selectedCategory.trim();
        const trimmedBrand = selectedBrand.trim();

        const hasCategoryOrBrand = !!(trimmedCategory || trimmedBrand);
        const hasSearch = trimmedSearch.length > 0;

        // No filters at all -> show all
        if (!hasSearch && !hasCategoryOrBrand) {
          setProducts(allProducts);
          return;
        }

        // Avoid noisy 1-character search unless category/brand filter is present
        if (hasSearch && trimmedSearch.length < 2 && !hasCategoryOrBrand) {
          setProducts(allProducts);
          return;
        }

        const data = await searchProducts({
          query: trimmedSearch.length >= 2 ? trimmedSearch : "",
          category: trimmedCategory,
          brand: trimmedBrand,
        });

        setProducts(data);
      } catch (err) {
        setError("Failed to search products.");
        console.error(err);
      } finally {
        setLoading(false);
      }
    };

    runSearch();
  }, [debouncedSearchTerm, selectedCategory, selectedBrand, allProducts]);

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

  const activeFilterCount =
    (searchTerm.trim() ? 1 : 0) +
    (selectedCategory ? 1 : 0) +
    (selectedBrand ? 1 : 0);

  return (
    <div className="catalog-page">
      <div className="bg-orb orb-left"></div>
      <div className="bg-orb orb-right"></div>
      <div className="bg-grid"></div>

      <header className="top-navbar">
        <div className="nav-inner">
          <div className="brand-wrap">
            <div className="brand-icon">S</div>
            <div>
              <h3>SmartSearch</h3>
              <span>Premium Product Discovery</span>
            </div>
          </div>

          <nav className="nav-links">
            <a href="#hero">Overview</a>
            <a href="#search">Search</a>
            <a href="#catalog">Catalog</a>
          </nav>

          <div className="nav-status">
            <span className="status-chip">Results {products.length}</span>
            <span className="status-chip accent-chip">
              Filters {activeFilterCount}
            </span>
          </div>
        </div>
      </header>

      <section className="hero" id="hero">
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

      <section className="search-shell glass-panel" id="search">
        <div className="search-top">
          <div>
            <h2>Search Catalog</h2>
            <p>{loading ? "Searching..." : `${products.length} results available`}</p>
          </div>

          <button
            className="clear-btn"
            onClick={() => {
              setSearchTerm("");
              setDebouncedSearchTerm("");
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
            <option value="">Category</option>
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
            <option value="">Brand</option>
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
        <section className="product-grid" id="catalog">
          {products.length === 0 ? (
            <div className="empty-state glass-panel">
              <h3>No products found</h3>
              <p>Try a different search term or clear the filters.</p>
              <button
                className="clear-btn"
                onClick={() => {
                  setSearchTerm("");
                  setDebouncedSearchTerm("");
                  setSelectedCategory("");
                  setSelectedBrand("");
                  setError("");
                }}
              >
                Clear Filters
              </button>
            </div>
          ) : (
            products.map((product) => (
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
            ))
          )}
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