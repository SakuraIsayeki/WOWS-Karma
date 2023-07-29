import argparse
import logging
from concurrent.futures import ThreadPoolExecutor
import http.client
import json
from argparse import ArgumentParser
from urllib.parse import urlparse
from jose import jwt


def get_connection(host: str) -> http.client.HTTPConnection:
    """Creates a connection to the API"""
    parsed_url = urlparse(host)
    clean_host = parsed_url.netloc or parsed_url.path
    return http.client.HTTPSConnection(clean_host) if host.startswith('https') \
        else http.client.HTTPConnection(clean_host)


class MinimapRenderClient:
    def __init__(self, host: str, token: str, error_output):
        self.host, self.token, self.error_output = host, token, error_output

    def fetch_posts_ids(self) -> list[str]:
        """Fetches all posts ids from the API"""
        # First get all posts
        connection = get_connection(self.host)
        request_url = f'{self.host}/api/post'
        connection.request('GET', request_url, headers={'Authorization': f'Bearer {self.token}'})
        response = connection.getresponse()

        # Deserialize response from JSON
        post_ids = json.loads(response.read().decode())
        logging.info(f'Found {len(post_ids)} posts on {self.host}')
        return post_ids

    def handle_error(self, response, post_id):
        """Handles an error response from the API"""
        # write error to file in json format into the current dictionary
        # key is the post id, value is the response body
        with open(self.error_output, 'a') as f:
            f.write(json.dumps({post_id: response.read().decode()})+'\n')

    def render_minimap(self, index, post_id, count):
        """Renders a minimap for a post by submitting a request to the API"""
        connection = get_connection(self.host)
        request_url = f'{self.host}/api/Replay/reprocess/minimap/{post_id}?waitForCompletion=true'
        connection.request('PATCH', request_url, headers={'Authorization': f'Bearer {self.token}'})
        response = connection.getresponse()

        if response.status == 200:
            logging.info(f'[{index + 1}/{count}] Rendered minimap for post {post_id}')
        else:
            logging.warning(f'[{index + 1}/{count}] Failed to render minimap for post {post_id}: {response.status} - {response.reason}')
            self.handle_error(response, post_id)
        return response.read()


def valid_uri(uri):
    """Validates a URI string"""
    try:
        result = urlparse(uri)
        return all([result.scheme, result.netloc])
    except ValueError:
        raise argparse.ArgumentTypeError(f'Invalid URI: {uri}')


def valid_jwt(token_string):
    """Validates a JWT string"""
    if len(token_string) > 0 and jwt.get_unverified_header(token_string) is not None:
        return token_string
    else:
        raise argparse.ArgumentTypeError(f'Invalid JWT: {token_string}')


def main():
    """Main entry point"""
    parser = ArgumentParser(description='Renders all posts minimaps')
    parser.add_argument('--host', required=True, type=str, help='API Host to connect to')
    parser.add_argument('--token', required=True, type=str, help='API JWT Auth Token to use')
    parser.add_argument('--concurrency', type=int, default=1, help='Number of parallel requests to make')
    parser.add_argument('--skip', type=int, default=0, help='Number of posts to skip')
    parser.add_argument('--error-output', type=str, default='error.json', help='File to write errors to')
    args = parser.parse_args()

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.FileHandler("debug.log"),
            logging.StreamHandler()
        ]
    )

    logging.info('Starting minimaps rendering...')

    client = MinimapRenderClient(args.host, args.token, args.error_output)

    # First get all posts
    posts = client.fetch_posts_ids()
    posts.reverse()

    if args.skip > 0:
        logging.info(f'Skipping {args.skip} posts out of {len(posts)} total results from the API')
        posts = posts[args.skip:]

    count = len(posts)

    # Then the fun part. Loop through all posts and render their minimaps by calling the API
    # Added caveat: Parallel requests are handled here, because the API is not able to handle them
    # This is a bit hacky, but it works
    with ThreadPoolExecutor(max_workers=args.concurrency) as executor:
        list(executor.map(lambda x: client.render_minimap(*x, count), enumerate(posts)))

    logging.info(f'Finished rendering {count} minimaps.')


if __name__ == "__main__":
    main()