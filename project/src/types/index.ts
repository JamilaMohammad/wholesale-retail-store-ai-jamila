export interface User {
  id: string;
  email: string;
  name: string;
  clientType: 'wholesaler' | 'retailer';
}

export interface Product {
  id: string;
  name: string;
  description: string;
  wholesalePrice: number;
  retailPrice: number;
  image: string;
  category: string;
  inStock: boolean;
}

export interface CartItem {
  productId: string;
  quantity: number;
  product: Product;
}

export interface AuthContextType {
  user: User | null;
  login: (email: string, password: string) => Promise<boolean>;
  signup: (email: string, password: string, name: string, clientType: 'wholesaler' | 'retailer') => Promise<boolean>;
  logout: () => void;
  loading: boolean;
}

export interface CartContextType {
  items: CartItem[];
  addToCart: (product: Product, quantity: number) => void;
  removeFromCart: (productId: string) => void;
  updateQuantity: (productId: string, quantity: number) => void;
  clearCart: () => void;
  getTotalPrice: (clientType: 'wholesaler' | 'retailer') => number;
  getItemCount: () => number;
}