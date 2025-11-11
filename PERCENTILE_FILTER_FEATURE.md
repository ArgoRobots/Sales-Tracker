# Dynamic Percentile-Based Thresholds for All Numerical Fields

## Overview

This feature comprehensively enhances the AI-powered search functionality by dynamically calculating context-aware thresholds for ALL numerical fields based on actual transaction data, rather than using fixed hardcoded values.

## Problem Solved

Previously, when users searched using qualitative terms in AI queries, the system used hardcoded thresholds that didn't reflect the actual data:
- `"expensive items"` → hardcoded $200 threshold
- `"high discount"` → hardcoded 5% threshold
- `"low price"` → hardcoded $50 threshold
- And so on...

This didn't account for:
- Businesses with very high-value transactions (where $200 might be considered cheap)
- Businesses with low-value transactions (where $200 might exclude everything)
- Different discount structures, shipping costs, and pricing scales
- Varying scales of operations across different business models

## Solution

The system now:
1. **Analyzes the current dataset** before processing AI queries across ALL numerical fields
2. **Calculates dynamic thresholds** based on percentiles of actual transaction values for each field
3. **Scales the percentile** based on the number of transactions:
   - Very few transactions (1-20): Uses 50th/50th percentile (top/bottom 50%)
   - Few transactions (21-50): Uses 75th/25th percentile (top/bottom 25%)
   - Medium transactions (51-200): Uses 90th/10th percentile (top/bottom 10%)
   - Many transactions (201-500): Uses 95th/5th percentile (top/bottom 5%)
   - Lots of transactions (500+): Uses 98th/2nd percentile (top/bottom 2%)

4. **Passes all thresholds to the AI** so it can accurately interpret qualitative terms for ANY field

## Supported Fields

The system now dynamically calculates thresholds for:
- **Total** (expensive/cheap, high cost/low cost)
- **Price per unit** (high price/low price)
- **Discount** (high discount/low discount)
- **Shipping** (high shipping/low shipping)
- **Tax** (high tax/low tax)
- **Fee** (high fee/low fee)
- **Quantity** (bulk orders/small orders, high quantity/low quantity)
- **Charged difference** (high/low variance)

## Example

If a business has 100 transactions:
- **Total values**: 90% are between $10-$100, top 10% are $100-$500
  - "expensive" → Total > ~$450 (90th percentile)
  - "cheap" → Total < ~$55 (10th percentile)

- **Discount values**: 90% are between 0%-10%, top 10% are 10%-50%
  - "high discount" → Discount > ~45% (90th percentile)
  - "low discount" → Discount < ~5% (10th percentile)

- **Shipping costs**: 90% are between $0-$10, top 10% are $10-$30
  - "high shipping" → Shipping > ~$27 (90th percentile)
  - "low shipping" → Shipping < ~$3 (10th percentile)

## Technical Implementation

### New Components

1. **`DynamicThresholds` class** (`Sales Tracker/Classes/DynamicThresholds.cs`)
   - Holds all calculated thresholds for high/low values across all numerical fields
   - `CreateDefault()`: Returns fallback default values
   - `ToString()`: Formatted output for logging

2. **Enhanced `PercentileCalculator` class** (`Sales Tracker/Classes/PercentileCalculator.cs`)
   - `CalculatePercentile()`: Calculates percentile values from a dataset using pure decimal arithmetic
   - `GetDynamicPercentileThreshold()`: Determines appropriate percentile based on transaction count
   - `GetColumnDecimalValues()`: Extracts decimal values from DataGridView columns
   - `CalculateColumnThresholds()`: Calculates both high and low thresholds for a specific column
   - `CalculateAllThresholds()`: **Main method** - calculates thresholds for all numerical fields at once
   - `GetExpensiveItemThreshold()`: Legacy method maintained for backwards compatibility

3. **Enhanced `AIQueryTranslator`** (`Sales Tracker/Classes/AIQueryTranslator.cs`)
   - Modified `TranslateQueryAsync()` to accept a `DynamicThresholds` parameter
   - Updated `BuildPrompt()` to use ALL dynamic thresholds in AI prompts
   - Now includes 20+ qualitative term mappings (expensive, cheap, high discount, low shipping, bulk, etc.)
   - Added comprehensive examples demonstrating multi-field threshold usage

4. **Enhanced `AISearchExtensions`** (`Sales Tracker/Classes/AISearchExtensions.cs`)
   - Modified `EnhanceSearchAsync()` to calculate ALL dynamic thresholds before translating queries
   - Calls `PercentileCalculator.CalculateAllThresholds()` to analyze all fields
   - Integrates with the selected DataGridView to analyze current data across all columns

### Tests

Comprehensive unit tests added in `Tests/PercentileCalculator_UnitTest.cs`:
- Percentile calculation accuracy with decimal precision
- Dynamic threshold scaling based on transaction count
- Multi-field threshold calculation
- DynamicThresholds class functionality
- Edge cases (empty data, null values, single values, missing columns)
- Integration with DataGridView for all supported fields
- Default fallback behavior

## Benefits

1. **Comprehensive context-aware filtering**: Qualitative terms for ALL fields are interpreted based on your actual data
2. **Automatic adaptation**: As the business scales and transaction values change, all thresholds adapt
3. **Better user experience**: More accurate results across all search dimensions without manual configuration
4. **Natural language queries**: Users can express complex queries naturally without knowing exact values
5. **Maintains backward compatibility**: Falls back to sensible defaults if data is unavailable
6. **Scalable intelligence**: Works for small businesses with 10 transactions or enterprises with 10,000+

## Usage Examples

No changes required for end users. Simply use natural language AI queries:

### Total/Cost Queries
- `!show me expensive items purchased last month`
- `!find cheap purchases from AliExpress`
- `!high cost sales to Europe`

### Discount Queries
- `!show items with high discounts`
- `!find purchases with low discounts last month`
- `!large discount sales to UK`

### Shipping Queries
- `!expensive shipping costs from China`
- `!items with low shipping or free shipping`
- `!high shipping sales to USA`

### Price Queries
- `!high price items with low quantity`
- `!cheap items with expensive shipping`
- `!low price bulk orders`

### Combined Queries
- `!expensive items with high discount and low shipping`
- `!bulk orders with low price per unit`
- `!small orders with high shipping costs`
- `!cheap items with high tax from Europe`

The system automatically calculates and applies appropriate thresholds for ALL fields based on your data!
