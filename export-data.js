// MongoDB Data Export Script
// Run this with: mongosh mongodb://localhost:27017/zah_ecommerce export-data.js

const fs = require('fs');

// Export Products
const products = db.products.find().toArray();
fs.writeFileSync('products.json', JSON.stringify(products, null, 2));
print(`Exported ${products.length} products`);

// Export Categories  
const categories = db.categories.find().toArray();
fs.writeFileSync('categories.json', JSON.stringify(categories, null, 2));
print(`Exported ${categories.length} categories`);

// Export Users
const users = db.users.find().toArray();
fs.writeFileSync('users.json', JSON.stringify(users, null, 2));
print(`Exported ${users.length} users`);

print('Export complete! Files saved in current directory');
