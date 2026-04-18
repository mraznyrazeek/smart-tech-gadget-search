import axios from "axios";

const API_BASE_URL = "http://localhost:5249/api"; // replace with your real backend HTTP port

export const getProducts = async () => {
  const response = await axios.get(`${API_BASE_URL}/products`);
  return response.data;
};

export const searchProducts = async ({ query = "", category = "", brand = "" }) => {
  const params = new URLSearchParams();

  if (query) params.append("query", query);
  if (category) params.append("category", category);
  if (brand) params.append("brand", brand);

  const response = await axios.get(`${API_BASE_URL}/products/search?${params.toString()}`);
  return response.data;
};