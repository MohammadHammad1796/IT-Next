const apiUrls = {
    contact: "contact",
    brands: "brands",
    contactMessages: "contact",
    hosting: "hosting",
    products: "products",
    subCategories: "subCategories",
    categories: "categories",
    settings: "settings"
}

const baseAppUrl = document.getElementsByTagName("base")[0].href;

for (let key in apiUrls)
    apiUrls[key] = `${baseAppUrl}api/${apiUrls[key]}/`;