import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

type TokenResponse = {
    accessToken: string;
    tokenType: string;
    expiresIn: number;
};

export default function LoginPage() {
    const navigate = useNavigate();

    const [email, setEmail] = useState("");
    const [password, setPassword] = useState("");
    const [errorMsg, setErrorMsg] = useState<string | null>(null);
    const [isLoading, setIsLoading] = useState(false);

    const onSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setErrorMsg(null);
        setIsLoading(true);

        try {
            const res = await fetch("/api/auth/login", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                // AuthService expects { username, password }
                body: JSON.stringify({ username: email, password }),
            });

            if (!res.ok) {
                setErrorMsg("Login failed. Please check email/password.");
                return;
            }

            const data = (await res.json()) as TokenResponse;

            // Store token for later calls (demo-friendly)
            localStorage.setItem("gtl_access_token", data.accessToken);
            localStorage.setItem("gtl_token_type", data.tokenType);
            localStorage.setItem("gtl_expires_in", String(data.expiresIn));

            // Route to next page
            navigate("/catalog");
        } catch {
            setErrorMsg("Login failed. API Gateway unreachable.");
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div style={{ minHeight: "100vh", display: "grid", placeItems: "center", padding: 24 }}>
            <div style={{ width: "100%", maxWidth: 420, padding: 24, border: "1px solid #ddd", borderRadius: 12 }}>
                <div style={{ marginBottom: 16 }}>
                    <div style={{ fontSize: 22, fontWeight: 700 }}>Georgia Tech Library</div>
                    <div style={{ color: "#666", marginTop: 4 }}>Demo login (admin / admin)</div>
                </div>

                <form onSubmit={onSubmit}>
                    <div style={{ display: "grid", gap: 12 }}>
                        <div style={{ display: "grid", gap: 6 }}>
                            <label style={{ fontWeight: 600 }}>Email</label>
                            <input
                                value={email}
                                onChange={(e) => setEmail(e.target.value)}
                                placeholder="admin"
                                autoComplete="username"
                                style={{ padding: 10, borderRadius: 8, border: "1px solid #ccc" }}
                            />
                        </div>

                        <div style={{ display: "grid", gap: 6 }}>
                            <label style={{ fontWeight: 600 }}>Password</label>
                            <input
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                placeholder="admin"
                                autoComplete="current-password"
                                style={{ padding: 10, borderRadius: 8, border: "1px solid #ccc" }}
                            />
                        </div>

                        {errorMsg && (
                            <div style={{ padding: 10, borderRadius: 8, background: "#fff3f3", border: "1px solid #ffd0d0" }}>
                                {errorMsg}
                            </div>
                        )}

                        <button
                            type="submit"
                            disabled={isLoading}
                            style={{
                                padding: 12,
                                borderRadius: 10,
                                border: "none",
                                fontWeight: 700,
                                cursor: isLoading ? "not-allowed" : "pointer",
                            }}
                        >
                            {isLoading ? "Signing in..." : "Sign in"}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
