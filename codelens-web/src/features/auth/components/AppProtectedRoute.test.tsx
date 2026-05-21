import { render, screen } from '@testing-library/react'
import { vi } from 'vitest'
import { MemoryRouter } from 'react-router-dom'
import AppProtectedRoute from './AppProtectedRoute'
import * as authHooks from '../hooks/authHooks'

vi.mock('../hooks/authHooks')

test('shows loading state', () => {
  vi.mocked(authHooks.useMe).mockReturnValue({ isPending: true, isError: false, data: undefined } as any)
  render(<MemoryRouter><AppProtectedRoute /></MemoryRouter>)
  expect(screen.getByText('Loading...')).toBeInTheDocument()
})

test('shows error state', () => {
  vi.mocked(authHooks.useMe).mockReturnValue({ isPending: false, isError: true, data: undefined } as any)
  render(<MemoryRouter><AppProtectedRoute /></MemoryRouter>)
  expect(screen.getByText('Something went worng')).toBeInTheDocument()
})
