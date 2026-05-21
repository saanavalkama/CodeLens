import { useAuthStore } from './authStore'

beforeEach(() => {
  useAuthStore.setState({ accessToken: null })
})

test('initial state has null token', () => {
  expect(useAuthStore.getState().accessToken).toBeNull()
})

test('setToken updates the access token', () => {
  useAuthStore.getState().setToken('abc123')
  expect(useAuthStore.getState().accessToken).toBe('abc123')
})

test('clearToken resets token to null', () => {
  useAuthStore.getState().setToken('abc123')
  useAuthStore.getState().clearToken()
  expect(useAuthStore.getState().accessToken).toBeNull()
})
