using System;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace VisualMetronome;

public class BpmScraperService
{
    private IWebDriver? _driver;
    private bool _isInitialized = false;

    public async Task<int?> GetBpmAsync(string songName, string artistName)
    {
        try
        {
            // Initialize driver if needed
            if (!_isInitialized)
            {
                InitializeDriver();
            }

            if (_driver == null)
            {
                Console.WriteLine("WebDriver failed to initialize");
                return null;
            }

            // Navigate to songbpm.com
            Console.WriteLine($"Navigating to songbpm.com...");
            await Task.Run(() => _driver.Navigate().GoToUrl("https://songbpm.com/"));

            // Wait for page to load and find search input
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
            
            Console.WriteLine("Waiting for search input field...");
            
            IWebElement? searchInput = null;
            
            try
            {
                // Try to find input field
                searchInput = wait.Until(driver =>
                {
                    try
                    {
                        var inputs = driver.FindElements(By.TagName("input"));
                        Console.WriteLine($"Found {inputs.Count} input elements");
                        
                        foreach (var input in inputs)
                        {
                            if (input.Displayed && input.Enabled)
                            {
                                var placeholder = input.GetDomAttribute("placeholder");
                                var type = input.GetDomAttribute("type");
                                Console.WriteLine($"Input: type='{type}', placeholder='{placeholder}'");
                                
                                if (type == "text" || type == "search" || placeholder?.Contains("song", StringComparison.OrdinalIgnoreCase) == true)
                                {
                                    Console.WriteLine("Found suitable search input!");
                                    return input;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error finding input: {ex.Message}");
                    }
                    
                    return null;
                });
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Timeout waiting for search input field");
                Console.WriteLine($"Current URL: {_driver.Url}");
                Console.WriteLine($"Page title: {_driver.Title}");
                return null;
            }

            if (searchInput == null)
            {
                Console.WriteLine("Could not find search input field");
                return null;
            }

            // Construct search query
            string searchQuery = $"{songName} {artistName}";
            Console.WriteLine($"Searching for: {searchQuery}");

            // Clear and enter search query
            searchInput.Clear();
            await Task.Delay(500);
            searchInput.SendKeys(searchQuery);
            await Task.Delay(500);
            
            // Submit the form (press Enter)
            Console.WriteLine("Submitting search...");
            searchInput.SendKeys(Keys.Return);

            // Wait for results page to load with explicit wait
            Console.WriteLine("Waiting for results page...");
            
            try
            {
                // Wait for URL to change or results to appear
                var resultsWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                resultsWait.Until(driver => 
                {
                    // Check if page has loaded by looking for content
                    try
                    {
                        var body = driver.FindElement(By.TagName("body"));
                        var text = body.Text;
                        // Look for signs the results have loaded
                        return text.Length > 500 && (text.Contains("BPM", StringComparison.OrdinalIgnoreCase) || 
                                                      text.Contains("Key", StringComparison.OrdinalIgnoreCase) ||
                                                      text.Contains("Duration", StringComparison.OrdinalIgnoreCase));
                    }
                    catch
                    {
                        return false;
                    }
                });
                
                Console.WriteLine("Results page loaded");
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("Timeout waiting for results page to load");
            }

            await Task.Delay(1000); // Give extra time for any JavaScript

            Console.WriteLine($"Results page URL: {_driver.Url}");
            Console.WriteLine("Searching for BPM value from first result...");

            // Extract BPM from the FIRST result in the results list
            var bpm = await Task.Run(() =>
            {
                try
                {
                    // Strategy: Find elements containing "BPM" text, then look for numeric values nearby
                    // This matches the structure: <span>BPM</span> followed by <span>93</span>
                    
                    // Find all elements that contain exactly "BPM" text (labels)
                    var bpmLabelElements = _driver.FindElements(By.XPath("//*[normalize-space(text())='BPM' or normalize-space(text())='bpm']"));
                    Console.WriteLine($"Found {bpmLabelElements.Count} elements with 'BPM' label");
                    
                    foreach (var labelElement in bpmLabelElements)
                    {
                        try
                        {
                            // Get the parent container
                            var parentContainer = labelElement.FindElement(By.XPath(".."));
                            Console.WriteLine($"Checking parent container for BPM label");
                            
                            // Look for sibling or child elements that contain just a number
                            var siblingElements = parentContainer.FindElements(By.XPath(".//*"));
                            
                            foreach (var sibling in siblingElements)
                            {
                                try
                                {
                                    string text = sibling.Text.Trim();
                                    // Skip the BPM label itself
                                    if (text.Equals("BPM", StringComparison.OrdinalIgnoreCase))
                                        continue;
                                    
                                    // Check if this element contains just a number (the BPM value)
                                    if (int.TryParse(text, out int bpmValue) && bpmValue >= 40 && bpmValue <= 300)
                                    {
                                        Console.WriteLine($"✓ Found BPM value '{bpmValue}' next to BPM label");
                                        return bpmValue;
                                    }
                                }
                                catch { }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error checking BPM label parent: {ex.Message}");
                        }
                    }
                    
                    // Fallback: Look for any span/div that contains just a 2-3 digit number near "BPM" text
                    Console.WriteLine("Trying fallback: looking for numeric patterns near BPM text");
                    
                    var allSpans = _driver.FindElements(By.TagName("span"));
                    bool foundBpmLabel = false;
                    
                    foreach (var span in allSpans)
                    {
                        try
                        {
                            string text = span.Text.Trim();
                            
                            // First, find a span that says "BPM"
                            if (text.Equals("BPM", StringComparison.OrdinalIgnoreCase))
                            {
                                foundBpmLabel = true;
                                Console.WriteLine("Found BPM label span");
                                continue;
                            }
                            
                            // If we found a BPM label, look for a numeric value in subsequent spans
                            if (foundBpmLabel && int.TryParse(text, out int bpmValue) && bpmValue >= 40 && bpmValue <= 300)
                            {
                                Console.WriteLine($"✓ Found BPM value '{bpmValue}' after BPM label");
                                return bpmValue;
                            }
                        }
                        catch { }
                    }
                    
                    // Final fallback: Get first number pattern after "BPM" in page text
                    Console.WriteLine("Using final fallback: regex on page text");
                    var pageText = _driver.FindElement(By.TagName("body")).Text;
                    
                    // Look for "BPM" followed by a number, or a number followed by "BPM"
                    var matches = System.Text.RegularExpressions.Regex.Matches(
                        pageText, 
                        @"BPM\s*(\d{2,3})|(\d{2,3})\s*BPM", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase
                    );
                    
                    if (matches.Count > 0)
                    {
                        var match = matches[0];
                        string bpmStr = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
                        if (int.TryParse(bpmStr, out int bpmValue) && bpmValue >= 40 && bpmValue <= 300)
                        {
                            Console.WriteLine($"✓ Found BPM from text pattern: {bpmValue}");
                            return bpmValue;
                        }
                    }
                    
                    Console.WriteLine("Could not extract BPM from page");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error extracting BPM: {ex.Message}");
                }
                
                return (int?)null;
            });

            return bpm;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scraping BPM: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return null;
        }
    }

    private void InitializeDriver()
    {
        try
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new"); // Run in headless mode
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
            options.AddArgument("--log-level=3"); // Suppress Chrome logs
            options.AddArgument("--silent");
            options.AddArgument("--disable-logging");
            
            // Suppress console errors
            options.AddExcludedArgument("enable-logging");

            Console.WriteLine("Initializing Chrome WebDriver (visible mode for debugging)...");
            
            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.HideCommandPromptWindow = true;
            
            _driver = new ChromeDriver(service, options);
            _isInitialized = true;
            Console.WriteLine("Chrome WebDriver initialized successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize WebDriver: {ex.Message}");
            _isInitialized = false;
        }
    }

    public void Dispose()
    {
        try
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
        catch { }
    }
}
