from flask import Flask, render_template, jsonify, request, session
from flask_cors import CORS
import json
import random
from datetime import datetime
import os
import threading
import time
from news_generator import NewsGenerator

app = Flask(__name__)
app.secret_key = 'lcn-exchange-secret-key-2025'
CORS(app)

# Load companies data
def load_companies():
    with open('companies.json', 'r', encoding='utf-8') as f:
        return json.load(f)

# Save companies data
def save_companies(companies):
    with open('companies.json', 'w', encoding='utf-8') as f:
        json.dump(companies, f, indent=2, ensure_ascii=False)

# Load news data
def load_news():
    with open('news.json', 'r', encoding='utf-8') as f:
        return json.load(f)

# Initialize companies
companies = load_companies()

# News storage (in-memory, will persist during app runtime)
active_news = []  # Latest news (max 10)
old_news = []     # Archived news

# Global money and portfolio (shared across all users)
global_cash = 1
global_portfolio = []

# Initialize news generator
try:
    news_generator = NewsGenerator()
    print("News generator loaded successfully!")
except Exception as e:
    print(f"Failed to load news generator: {e}")
    news_generator = None

# Load initial news
initial_news = load_news()
active_news = initial_news[:10] if len(initial_news) > 10 else initial_news
old_news = initial_news[10:] if len(initial_news) > 10 else []

# Money and portfolio functions (global system)
def get_global_money():
    global global_cash
    return global_cash

def set_global_money(amount):
    global global_cash
    global_cash = amount

def add_global_money(amount):
    global global_cash
    global_cash += amount

def subtract_global_money(amount):
    global global_cash
    global_cash -= amount
    
def get_global_portfolio():
    global global_portfolio
    return global_portfolio

@app.route('/')
def index():
    """News homepage"""
    return render_template('news.html')

@app.route('/markets')
def markets():
    """Markets page"""
    return render_template('markets.html')

@app.route('/api/news', methods=['GET'])
def get_news():
    """Get active news articles (latest 10)"""
    return jsonify(active_news)

@app.route('/api/news/old', methods=['GET'])
def get_old_news():
    """Get archived old news"""
    return jsonify(old_news)

@app.route('/api/news/<int:news_id>', methods=['GET'])
def get_news_detail(news_id):
    """Get specific news article"""
    all_news = active_news + old_news
    article = next((n for n in all_news if n['id'] == news_id), None)
    if article:
        return jsonify(article)
    return jsonify({'error': 'News article not found'}), 404

@app.route('/api/companies', methods=['GET'])
def get_companies():
    """Get all companies"""
    return jsonify(companies)

@app.route('/api/company/<ticker>', methods=['GET'])
def get_company(ticker):
    """Get specific company by ticker"""
    company = next((c for c in companies if c['ticker'] == ticker), None)
    if company:
        return jsonify(company)
    return jsonify({'error': 'Company not found'}), 404

@app.route('/api/portfolio', methods=['GET'])
def get_portfolio():
    """Get global portfolio"""
    portfolio_with_details = []
    
    for item in get_global_portfolio():
        company = next((c for c in companies if c['ticker'] == item['ticker']), None)
        if company:
            portfolio_with_details.append({
                'ticker': item['ticker'],
                'name': company['name'],
                'shares': item['shares'],
                'avgPrice': item['avgPrice'],
                'currentPrice': company['currentPrice'],
                'profit': (company['currentPrice'] - item['avgPrice']) * item['shares'],
                'profitPercent': ((company['currentPrice'] - item['avgPrice']) / item['avgPrice'] * 100)
            })
    
    return jsonify({
        'cash': get_global_money(),
        'portfolio': portfolio_with_details,
        'totalValue': get_global_money() + sum(item['currentPrice'] * item['shares'] for item in portfolio_with_details)
    })

@app.route('/api/cash', methods=['GET'])
def get_cash():
    """Get global cash"""
    return jsonify({'cash': get_global_money()})

@app.route('/api/trade', methods=['POST'])
def trade():
    """Execute a trade (buy/sell)"""
    global global_portfolio
    data = request.json
    
    action = data.get('action')  # 'buy' or 'sell'
    ticker = data.get('ticker')
    shares = int(data.get('shares', 0))
    
    if shares <= 0:
        return jsonify({'error': 'Invalid number of shares'}), 400
    
    company = next((c for c in companies if c['ticker'] == ticker), None)
    if not company:
        return jsonify({'error': 'Company not found'}), 404
    
    if action == 'buy':
        cost = shares * company['currentPrice']
        
        if cost > get_global_money():
            return jsonify({'error': 'Insufficient funds'}), 400
        
        # Deduct cash
        subtract_global_money(cost)
        
        # Update portfolio
        portfolio = get_global_portfolio()
        existing = next((p for p in portfolio if p['ticker'] == ticker), None)
        
        if existing:
            total_shares = existing['shares'] + shares
            total_cost = (existing['avgPrice'] * existing['shares']) + cost
            existing['avgPrice'] = total_cost / total_shares
            existing['shares'] = total_shares
        else:
            portfolio.append({
                'ticker': ticker,
                'shares': shares,
                'avgPrice': company['currentPrice']
            })
        
        return jsonify({
            'success': True,
            'message': f'Successfully bought {shares} shares of {ticker}',
            'cash': get_global_money()
        })
    
    elif action == 'sell':
        portfolio = get_global_portfolio()
        existing = next((p for p in portfolio if p['ticker'] == ticker), None)
        
        if not existing or existing['shares'] < shares:
            return jsonify({'error': 'Insufficient shares'}), 400
        
        revenue = shares * company['currentPrice']
        
        # Add cash
        add_global_money(revenue)
        
        # Update portfolio
        existing['shares'] -= shares
        if existing['shares'] == 0:
            portfolio.remove(existing)
        
        return jsonify({
            'success': True,
            'message': f'Successfully sold {shares} shares of {ticker}',
            'cash': get_global_money()
        })
    
    return jsonify({'error': 'Invalid action'}), 400

@app.route('/api/update-prices', methods=['POST'])
def update_prices():
    """Update all stock prices (simulate market movement)"""
    global companies
    
    for company in companies:
        # Store previous price
        company['previousPrice'] = company['currentPrice']
        
        # Random price change (-5% to +5%)
        change_percent = (random.random() - 0.5) * 0.1
        new_price = company['currentPrice'] * (1 + change_percent)
        company['currentPrice'] = max(round(new_price, 2), 1)  # Minimum $1
        
        # Update price history
        company['priceHistory'].pop(0)
        company['priceHistory'].append(company['currentPrice'])
    
    # Save updated prices
    save_companies(companies)
    
    return jsonify({'success': True, 'companies': companies})

@app.route('/api/reset', methods=['POST'])
def reset_portfolio():
    """Reset portfolio and cash"""
    global global_portfolio
    set_global_money(1)
    global_portfolio = []
    
    return jsonify({
        'success': True,
        'message': 'Portfolio reset successfully',
        'cash': get_global_money()
    })

@app.route('/api/stats', methods=['GET'])
def get_stats():
    """Get market statistics"""
    total_companies = len(companies)
    gainers = sum(1 for c in companies if c['currentPrice'] > c['previousPrice'])
    losers = sum(1 for c in companies if c['currentPrice'] < c['previousPrice'])
    unchanged = total_companies - gainers - losers
    
    return jsonify({
        'total_companies': total_companies,
        'gainers': gainers,
        'losers': losers,
        'unchanged': unchanged
    })

# ==================== UNITY API ENDPOINTS ====================

@app.route('/api/unity/money/check', methods=['GET'])
def unity_check_money():
    """Unity: Check global money"""
    return jsonify({
        'success': True,
        'money': get_global_money(),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/unity/money/add', methods=['POST'])
def unity_add_money():
    """Unity: Add money globally"""
    data = request.json
    amount = float(data.get('amount', 0))
    
    if amount <= 0:
        return jsonify({
            'success': False,
            'error': 'Amount must be positive'
        }), 400
    
    previous_money = get_global_money()
    add_global_money(amount)
    
    return jsonify({
        'success': True,
        'message': f'Added ${amount:,.2f}',
        'previousMoney': previous_money,
        'currentMoney': get_global_money(),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/unity/money/subtract', methods=['POST'])
def unity_subtract_money():
    """Unity: Subtract money globally"""
    data = request.json
    amount = float(data.get('amount', 0))
    
    if amount <= 0:
        return jsonify({
            'success': False,
            'error': 'Amount must be positive'
        }), 400
    
    if amount > get_global_money():
        return jsonify({
            'success': False,
            'error': 'Insufficient funds',
            'currentMoney': get_global_money(),
            'requestedAmount': amount
        }), 400
    
    previous_money = get_global_money()
    subtract_global_money(amount)
    
    return jsonify({
        'success': True,
        'message': f'Subtracted ${amount:,.2f}',
        'previousMoney': previous_money,
        'currentMoney': get_global_money(),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/unity/money/set', methods=['POST'])
def unity_set_money():
    """Unity: Set global money to specific amount"""
    data = request.json
    amount = float(data.get('amount', 0))
    
    if amount < 0:
        return jsonify({
            'success': False,
            'error': 'Amount cannot be negative'
        }), 400
    
    previous_money = get_global_money()
    set_global_money(amount)
    
    return jsonify({
        'success': True,
        'message': f'Money set to ${amount:,.2f}',
        'previousMoney': previous_money,
        'currentMoney': get_global_money(),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/unity/health', methods=['GET'])
def unity_health():
    """Unity: Check Flask server health"""
    return jsonify({
        'success': True,
        'status': 'running',
        'server': 'Flask Trading API',
        'version': '1.0.0',
        'timestamp': datetime.now().isoformat()
    })

# ==================== END UNITY API ENDPOINTS ====================

# ==================== MONEY MANAGEMENT API ENDPOINTS ====================

@app.route('/api/money/get', methods=['GET'])
def api_get_money():
    """API: Get current money amount"""
    return jsonify({
        'success': True,
        'money': get_global_money(),
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/money/add', methods=['POST'])
def api_add_money():
    """API: Add money"""
    data = request.json
    
    if not data:
        return jsonify({
            'success': False,
            'error': 'No data provided'
        }), 400
    
    amount = data.get('amount')
    
    if amount is None:
        return jsonify({
            'success': False,
            'error': 'Amount is required'
        }), 400
    
    try:
        amount = float(amount)
    except (ValueError, TypeError):
        return jsonify({
            'success': False,
            'error': 'Amount must be a valid number'
        }), 400
    
    if amount <= 0:
        return jsonify({
            'success': False,
            'error': 'Amount must be positive'
        }), 400
    
    previous_money = get_global_money()
    add_global_money(amount)
    
    return jsonify({
        'success': True,
        'message': f'Successfully added {amount}',
        'previousMoney': previous_money,
        'currentMoney': get_global_money(),
        'addedAmount': amount,
        'timestamp': datetime.now().isoformat()
    })

@app.route('/api/money/subtract', methods=['POST'])
def api_subtract_money():
    """API: Subtract money"""
    data = request.json
    
    if not data:
        return jsonify({
            'success': False,
            'error': 'No data provided'
        }), 400
    
    amount = data.get('amount')
    
    if amount is None:
        return jsonify({
            'success': False,
            'error': 'Amount is required'
        }), 400
    
    try:
        amount = float(amount)
    except (ValueError, TypeError):
        return jsonify({
            'success': False,
            'error': 'Amount must be a valid number'
        }), 400
    
    if amount <= 0:
        return jsonify({
            'success': False,
            'error': 'Amount must be positive'
        }), 400
    
    current_money = get_global_money()
    
    if amount > current_money:
        return jsonify({
            'success': False,
            'error': 'Insufficient funds',
            'currentMoney': current_money,
            'requestedAmount': amount
        }), 400
    
    previous_money = current_money
    subtract_global_money(amount)
    
    return jsonify({
        'success': True,
        'message': f'Successfully subtracted {amount}',
        'previousMoney': previous_money,
        'currentMoney': get_global_money(),
        'subtractedAmount': amount,
        'timestamp': datetime.now().isoformat()
    })

# ==================== END MONEY MANAGEMENT API ENDPOINTS ====================

if __name__ == '__main__':
    # Start background news generation
    def generate_news_periodically():
        """Generate news every 30 seconds"""
        while True:
            try:
                time.sleep(30)  # Wait 30 seconds
                
                if news_generator:
                    # Generate new news
                    new_article = news_generator.generate_news()
                    
                    # Add to active news at the beginning
                    active_news.insert(0, new_article)
                    
                    # If active news exceeds 10, move oldest to old_news
                    if len(active_news) > 10:
                        moved_article = active_news.pop()
                        old_news.insert(0, moved_article)
                    
                    print(f"Generated news: {new_article['title'][:50]}...")
                    
            except Exception as e:
                print(f"Error generating news: {e}")
    
    # Start background thread
    if news_generator:
        news_thread = threading.Thread(target=generate_news_periodically, daemon=True)
        news_thread.start()
        print("News generation background thread started!")
    
    app.run(debug=True, host='0.0.0.0', port=5000, use_reloader=False)
