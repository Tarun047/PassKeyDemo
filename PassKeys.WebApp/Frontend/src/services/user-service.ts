import {sessionStorageService} from "./session-storage-service.ts";
import {SessionConstants} from "../../constants.ts";
import {format} from "date-fns";

export class Credential {
    id!: string
    createdAtUtc!: string;
    updatedAtUtc!: string;
    lastUsedPlatformInfo!: string;
}

export class User {
    userName!: string
    credentials!: Credential[]
}

class UserService {
    async getUser() {
        const token = sessionStorageService.get<string>(SessionConstants.TokenKey);
        if (token === null) {
            throw new Error("Token expired!");
        }

        const response = await fetch('/api/users/me', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${token}`
            },
        });
        if (response.status != 200) {
            throw new Error(await response.text());
        }

        const user =  (await response.json()) as User;
        const dateFormat = 'dd-MMM-yyyy HH:mm aa'
        user.credentials = user.credentials.map(credential => {
            try {
                credential.createdAtUtc = format(credential.createdAtUtc, dateFormat);
                credential.updatedAtUtc = format(credential.updatedAtUtc, dateFormat);
            }
            catch(e) {
                console.log(e)
            }
            return credential;
        })
        
        return user;
    }
}

const userService = new UserService();
export {userService}