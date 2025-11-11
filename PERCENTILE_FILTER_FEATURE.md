# Dynamic Percentile-Based "Expensive Items" Filter

## Overview

This feature enhances the AI-powered search functionality by dynamically calculating what constitutes an "expensive" item based on the actual transaction data, rather than using a fixed threshold.

## Problem Solved

Previously, when users searched for "expensive items" using AI queries (e.g., `!show me expensive items purchased last month`), the system used a hardcoded threshold of $200. This didn't account for:
- Businesses with very high-value transactions (where $200 might be considered cheap)
- Businesses with low-value transactions (where $200 might exclude everything)
- Varying scales of operations

## Solution

The system now:
1. **Analyzes the current dataset** before processing AI queries
2. **Calculates a dynamic threshold** based on percentiles of actual transaction values
3. **Scales the percentile** based on the number of transactions:
   - Very few transactions (1-20): Uses top 50% (percentile 50)
   - Few transactions (21-50): Uses top 25% (percentile 75)
   - Medium transactions (51-200): Uses top 10% (percentile 90)
   - Many transactions (201-500): Uses top 5% (percentile 95)
   - Lots of transactions (500+): Uses top 2% (percentile 98)

4. **Passes this threshold to the AI** so it can accurately filter "expensive" items

## Example

If a business has 100 transactions with the following Total values:
- 90% of transactions are between $10-$100
- Top 10% are between $100-$500

The system will calculate the 90th percentile (since there are 100 transactions) as approximately $100. When a user searches for "expensive items", the AI will filter for items with Total > $100, rather than the previous hardcoded $200.

## Technical Implementation

### New Components

1. **`PercentileCalculator` class** (`Sales Tracker/Classes/PercentileCalculator.cs`)
   - `CalculatePercentile()`: Calculates percentile values from a dataset
   - `GetDynamicPercentileThreshold()`: Determines appropriate percentile based on transaction count
   - `GetColumnDecimalValues()`: Extracts decimal values from DataGridView columns
   - `GetExpensiveItemThreshold()`: Main method that calculates the threshold for "expensive" items

2. **Enhanced `AIQueryTranslator`** (`Sales Tracker/Classes/AIQueryTranslator.cs`)
   - Modified `TranslateQueryAsync()` to accept an optional `expensiveThreshold` parameter
   - Updated `BuildPrompt()` to use dynamic threshold in AI prompts instead of hardcoded value

3. **Enhanced `AISearchExtensions`** (`Sales Tracker/Classes/AISearchExtensions.cs`)
   - Modified `EnhanceSearchAsync()` to calculate dynamic threshold before translating queries
   - Integrates with the selected DataGridView to analyze current data

### Tests

Comprehensive unit tests added in `Tests/PercentileCalculator_UnitTest.cs`:
- Percentile calculation accuracy
- Dynamic threshold scaling based on transaction count
- Edge cases (empty data, null values, single values)
- Integration with DataGridView

## Benefits

1. **Context-aware filtering**: "Expensive" means different things for different businesses
2. **Automatic adaptation**: As the business scales and transaction values change, the filter adapts
3. **Better user experience**: More accurate results without manual threshold configuration
4. **Maintains backward compatibility**: Falls back to $200 if data is unavailable

## Usage

No changes required for end users. Simply use AI queries as before:
- `!show me expensive items purchased last month`
- `!find expensive purchases from AliExpress`
- `!expensive sales to Europe`

The system will automatically calculate and apply the appropriate threshold based on your data.
