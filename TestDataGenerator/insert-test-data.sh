#!/bin/bash
# Test data insertion commands for ti8m BeachBreak API
# Make sure the CommandApi is running on the expected port

# Set the base URL (adjust port if necessary)
BASE_URL="https://localhost:7001"
API_VERSION="1.0"

echo "Inserting organizations..."
curl -X POST "$BASE_URL/c/api/v$API_VERSION/organizations/bulk-import" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d @test-organizations.json

echo "Waiting 2 seconds..."
sleep 2

echo "Inserting employees..."
curl -X POST "$BASE_URL/c/api/v$API_VERSION/employees/bulk-insert" \
  -H "Content-Type: application/json" \
  -H "Accept: application/json" \
  -d @test-employees.json

echo "Test data insertion completed!"
