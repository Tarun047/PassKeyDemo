<template>
  <v-app>
    <v-layout>
      <v-app-bar color="secondary">
        <v-app-bar-title>Pass Key Demo</v-app-bar-title>
      </v-app-bar>
      <v-main>
        <v-snackbar border="start" v-model="snackBarModel" :color="snackbarColor" variant="elevated"
                    :text="snackbarMessage"
                    rounded timer>
          <template v-slot:actions>
            <v-btn icon="mdi-close" @click="snackBarModel = false">
            </v-btn>
          </template>
        </v-snackbar>
        <v-container>
          <v-col cols="12" v-if="!isLoggedIn">
            <v-row>
              <v-text-field variant="outlined" label="Username" v-model="userName"></v-text-field>
            </v-row>
            <v-row class="pt-2 d-flex justify-center">
              <v-btn color="primary" @click="createLoginPassKey">
                Create PassKey Login
              </v-btn>
            </v-row>
            <v-row class="pt-2 d-flex justify-center">
              <v-divider></v-divider>
              <v-label>OR</v-label>
            </v-row>
            <v-row class="pt-4 d-flex justify-center">
              <v-btn color="primary" variant="outlined" @click="loginWithPassKeys">
                Login with PassKeys
              </v-btn>
            </v-row>
          </v-col>
          <v-col v-else>
            <v-row class="d-flex justify-center text-lg-h4">
              Hola, {{ user.userName }}!
            </v-row>
            <v-row class="d-flex justify-center">
              <v-card flat title="User Passkeys">
                <template v-slot:text>
                  <v-text-field
                      v-model="search"
                      label="Search"
                      prepend-inner-icon="mdi-magnify"
                      single-line
                      variant="outlined"
                      hide-details
                  ></v-text-field>
                </template>
                <v-card-actions>
                  <v-btn color="primary" variant="outlined" @click="registerAdditionalPassKey">Add a Passkey</v-btn>
                </v-card-actions>
                <v-data-table
                    :search="search"
                    :items="user.credentials"
                    :headers="headers">
                  <template v-slot:item.actions="{ item }">
                    <v-icon
                        size="small"
                        @click="revokePassKey(item.id)"
                    >
                      mdi-delete
                    </v-icon>
                  </template>
                </v-data-table>
              </v-card>
            </v-row>
            <v-row class="d-flex pt-2 justify-center">
              <v-btn color="primary" variant="outlined" @click="logout">
                Logout
              </v-btn>
            </v-row>
          </v-col>
        </v-container>
      </v-main>
    </v-layout>
  </v-app>
</template>

<script setup lang="ts">

import {ref, computed, onMounted} from "vue";
import {passKeyService} from "./services/passkey-service.ts";
import {sessionStorageService} from "./services/session-storage-service.ts";
import {SessionConstants} from "../constants.ts";
import {User, userService} from "./services/user-service.ts";

const headers = [
  {title: 'Id', value: 'id'},
  {title: 'Creation Time', value: 'createdAtUtc'},
  {title: 'Last Use Time', value: 'updatedAtUtc'},
  {title: 'Last Used on', value: 'lastUsedPlatformInfo'},
  {title: 'Actions', key: 'actions', sortable: false},
]
const isLoggedIn = ref(sessionStorageService.get<string>(SessionConstants.TokenKey) != null);
const search = ref("");
const userName = ref("");
const snackbarMessage = ref("")
const snackbarColor = ref("success");
const user = ref<User>(new User());
const snackBarModel = computed({
  get() {
    return snackbarMessage.value.length > 0;
  },
  set() {
    snackbarMessage.value = "";
  }
});


async function refreshUser(): Promise<void> {
  const loggedInUser = await userService.getUser()
  console.log(loggedInUser);
  user.value = loggedInUser;
}

onMounted(() => {
  if (isLoggedIn.value === true) {
    refreshUser();
  }
})

function validateInputs(): boolean {
  if (userName.value == null || userName.value.length === 0) {
    snackbarColor.value = "error";
    snackbarMessage.value = "Username can't be empty!";
    return false;
  }


  return true;
}

function logout() {
  sessionStorageService.clear(SessionConstants.TokenKey);
  isLoggedIn.value = false;
}

function handleError(error: Error) {
  snackbarColor.value = "error";
  snackbarMessage.value = error.message;
}

function handleLoginSuccess() {
  snackbarColor.value = "success"
  snackbarMessage.value = `Successfully logged in as ${userName.value}`
}

async function createLoginPassKey() {
  if (!validateInputs()) {
    return
  }
  try {
    const credentialOptions = await passKeyService.createCredentialOptions(userName.value);
    const result = await passKeyService.createCredential(credentialOptions.userId, credentialOptions.options) as {
      credentialMakeResult: any,
      token: string
    }
    const credentialMakeResult = result.credentialMakeResult;
    if (credentialMakeResult.status === 'ok') {
      sessionStorageService.set(SessionConstants.TokenKey, result.token, SessionConstants.TokenExpiryTime);
      await refreshUser();
      isLoggedIn.value = true;
      handleLoginSuccess();
    }
  } catch (error: any) {
    handleError(error);
  }
}

async function registerAdditionalPassKey() {
  try {
    const credentialOptions = await passKeyService.createCredentialOptionsForCurrentUser()
    await passKeyService.createCredential(credentialOptions.userId, credentialOptions.options);
    await refreshUser();
  } catch (e: any) {
    handleError(e);
  }
}

async function loginWithPassKeys() {
  if (!validateInputs()) {
    return
  }

  try {
    const assertionOptions = await passKeyService.createAssertionOptions(userName.value);
    const result = await passKeyService.verifyAssertion(assertionOptions.userId, assertionOptions.options) as {
      assertionVerificationResult: any,
      token: string
    };
    const assertionResult = result.assertionVerificationResult;
    if (assertionResult.status === 'ok') {
      sessionStorageService.set(SessionConstants.TokenKey, result.token, SessionConstants.TokenExpiryTime);
      await refreshUser();
      isLoggedIn.value = true;
      handleLoginSuccess();
    }
  } catch (error: any) {
    handleError(error);
  }
}

async function revokePassKey(credentialId: string) {
  try {
    await passKeyService.revokeCredential(credentialId);
    await refreshUser();
  } catch (error: any) {
    handleError(error);
  }
}

</script>
