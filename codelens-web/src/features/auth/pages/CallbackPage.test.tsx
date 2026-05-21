import { render, waitFor } from '@testing-library/react'
import { vi } from 'vitest'
import CallbackPage from './CallbackPage'
import { authService } from '../services/authService'

const mockNavigate = vi.fn()

vi.mock('react-router-dom', () => ({
  useNavigate: () => mockNavigate,
}))

vi.mock('../services/authService', () => ({
  authService: {
    refreshToken: vi.fn(),
  },
}))

beforeEach(() => {
  mockNavigate.mockClear()
  vi.mocked(authService.refreshToken).mockClear()
})

test('navigates to /app on successful token refresh', async () => {
  vi.mocked(authService.refreshToken).mockResolvedValue({ accessToken: 'token123' })
  render(<CallbackPage />)
  await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith('/app'))
})

test('navigates to /login on failed token refresh', async () => {
  vi.mocked(authService.refreshToken).mockRejectedValue(new Error('unauthorized'))
  render(<CallbackPage />)
  await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith('/login'))
})
