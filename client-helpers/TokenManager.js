// Client-side token management helper
class TokenManager {
    constructor(baseUrl) {
        this.baseUrl = baseUrl;
        this.token = localStorage.getItem('authToken');
        this.refreshTimer = null;
        this.isRefreshing = false;
    }

    setToken(token) {
        this.token = token;
        localStorage.setItem('authToken', token);
        this.scheduleTokenRefresh();
    }

    getToken() {
        return this.token;
    }

    clearToken() {
        this.token = null;
        localStorage.removeItem('authToken');
        if (this.refreshTimer) {
            clearTimeout(this.refreshTimer);
            this.refreshTimer = null;
        }
    }

    // Schedule token refresh 30 minutes before expiry (7.5 hours after creation)
    scheduleTokenRefresh() {
        if (this.refreshTimer) {
            clearTimeout(this.refreshTimer);
        }

        // Refresh token after 7.5 hours (30 minutes before 8-hour expiry)
        const refreshTime = 7.5 * 60 * 60 * 1000; // 7.5 hours in milliseconds
        
        this.refreshTimer = setTimeout(async () => {
            await this.refreshToken();
        }, refreshTime);
    }

    async refreshToken() {
        if (this.isRefreshing || !this.token) {
            return false;
        }

        this.isRefreshing = true;

        try {
            const response = await fetch(`${this.baseUrl}/api/RefreshToken`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.token}`,
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                const data = await response.json();
                this.setToken(data.token);
                console.log('Token refreshed successfully');
                return true;
            } else {
                console.error('Token refresh failed:', response.status);
                this.handleTokenExpiry();
                return false;
            }
        } catch (error) {
            console.error('Token refresh error:', error);
            this.handleTokenExpiry();
            return false;
        } finally {
            this.isRefreshing = false;
        }
    }

    handleTokenExpiry() {
        this.clearToken();
        // Redirect to login or show login modal
        window.location.href = '/login';
    }

    // Enhanced fetch wrapper with automatic token refresh
    async authenticatedFetch(url, options = {}) {
        const makeRequest = async (token) => {
            return fetch(url, {
                ...options,
                headers: {
                    ...options.headers,
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                }
            });
        };

        let response = await makeRequest(this.token);

        // If token expired, try to refresh and retry once
        if (response.status === 401 && !this.isRefreshing) {
            const refreshed = await this.refreshToken();
            if (refreshed) {
                response = await makeRequest(this.token);
            }
        }

        return response;
    }

    // Initialize token manager on page load
    init() {
        if (this.token) {
            // Check if token is still valid by making a test request
            this.authenticatedFetch(`${this.baseUrl}/api/GetUser`)
                .then(response => {
                    if (response.ok) {
                        this.scheduleTokenRefresh();
                    } else if (response.status === 401) {
                        this.handleTokenExpiry();
                    }
                })
                .catch(error => {
                    console.error('Token validation error:', error);
                });
        }
    }
}

export default TokenManager;