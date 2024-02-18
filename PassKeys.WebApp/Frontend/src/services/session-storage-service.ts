interface IStoredItemWithExpiry<T> {
    item: T,
    expiry: number | null
}

class SessionStorageService {
    get<T>(key: string): T | null {
        const item = sessionStorage.getItem(key);
        if (item == null) {
            return null
        }
        const storedItem = JSON.parse(item) as IStoredItemWithExpiry<T>;
        console.log(storedItem)
        console.log(typeof storedItem)
        if (storedItem.expiry === null) {
            return storedItem.item;
        }

        if (storedItem.expiry > Date.now()) {
            return storedItem.item
        }

        sessionStorage.removeItem(key);

        return null;
    }

    set<T>(key: string, item: T, expiryInMilliSeconds: null | number = null) {
        let expiry = null;
        if (expiryInMilliSeconds !== null) {
            expiry = Date.now() + expiryInMilliSeconds;
        }
        sessionStorage.setItem(key, JSON.stringify({
            item,
            expiry
        } as IStoredItemWithExpiry<T>))
    }
    
    clear(key: string) {
        sessionStorage.removeItem(key);
    }
}

const sessionStorageService = new SessionStorageService();
export {sessionStorageService};