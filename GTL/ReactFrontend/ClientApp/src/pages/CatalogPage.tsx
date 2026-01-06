import React from "react";

export default function CatalogPage() {
    const token = localStorage.getItem("gtl_access_token");

    return (
        <div style={{ padding: 24 }}>
            <h2>Catalog</h2>
            <p>Login successful. We will build this page next.</p>
            <p>
                Token stored: <b>{token ? "Yes" : "No"}</b>
            </p>
        </div>
    );
}
